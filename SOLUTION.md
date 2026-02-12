# Solution Architecture & Design

## Entity Relationship Diagram

![Entity Relationship Diagram](Design/entity-relationship-diagram.png)

## Architecture Overview

**Onion Architecture** focuses on structuring layers around a central domain model, while **Clean Architecture** organises code around use cases explicitly defining what the application does.

For a loyalty points service, the value is in well-defined business workflows: **ingest a purchase → queue it → calculate points → update the ledger and balance**.

### Layer Structure

```
Domain (entities, interfaces)
    ↑
Application (services, MediatR handlers)
    ↑
Infrastructure (EF Core, repositories, messaging)
    ↑
API / Worker (controllers, background services)
```

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Entities (Purchase, PointsLedgerEntry, CustomerBalance, Merchant, Customer), interfaces, strategies. Zero external dependencies. |
| **Application** | MediatR commands/queries, pipeline behaviors, validators, response models. Depends only on Domain. |
| **Infrastructure** | EF Core DbContexts, repositories, messaging providers (SQS, InMemory, DevEmulator). |
| **API/Worker** | REST controllers, health checks, background queue consumers. |

---

## Design Patterns

### Command Query Responsibility Segregation (CQRS)

**Definition:** A behavioral design pattern that turns a request into a stand-alone object that contains all information about the request. This transformation lets you pass requests as method arguments, delay or queue a request's execution, and support undoable operations.

**Benefits:**
- **Scalability:** Scale reads (e.g., balance queries) independently from writes (purchase ingestion) for Zapper's bursts.
- **Simplicity:** Commands focus solely on state changes (idempotent points awards); queries return tailored DTOs without business logic.
- **Flexibility:** Use different DBs/stores—write-optimized relational for ledgers, read-optimized views for balances.

### MediatR (Mediator Pattern)

**Definition:** A behavioral design pattern that lets you reduce chaotic dependencies between objects. The pattern restricts direct communications between the objects and forces them to collaborate only via a mediator object.

**Benefits:**
- **Decoupling:** Controllers send commands without knowing handlers—easy to add validation, logging behaviors.
- **Testability:** Mock IMediator; unit test handlers in isolation.
- **Extensibility:** Pipeline behaviors handle cross-cutting concerns (AWS SQS enqueue, retries) globally.

**MediatR Pipeline Flow:**
```
Controller → LoggingBehavior → ValidationBehavior → Handler
```

**Important Note:** Using these patterns provides a reusable way to execute commands from different sources (API and Queue). We can pipe logging and validation behavior to ensure they always behave the same no matter the source, and this also allows for clear undoable operations. 

⚠️ **Caution:** Don't overuse the mediator pattern—the goal is to write less code, not create more spaghetti code dependencies. It's a good idea to reuse the mediator pattern by creating orchestrations of mediators rather than chains.

---

## Queue-Driven Processing (AWS SQS)

### Compute & Evolving Traffic

**Time Complexity:**
- **O(1) / O(10)** or **O(n / k)**
  - `n` = total messages in queue
  - `k` = number of worker tasks (2-10)

**Key Features:**
- **Event-driven architecture** — Decouples ingestion from processing
- **At-least-once delivery** — Auto retries failed messages to DLQ (Dead Letter Queue)
- **Auto-scaling** — Scale workers based on queue depth and CPU
- **Cost efficient** — Pay only for messages processed
- **FIFO support** — Optional strict ordering for serial execution
- **Unlimited scale** — SQS has virtually unlimited producers/consumers for concurrent connections
- **Batch operations** — Max 10 messages per batch (SendMessageBatch, ReceiveMessage, DeleteMessageBatch) = 60% more cost efficient than individual messages
- **Burst handling** — Absorbs traffic spikes; API responds quickly while workers process at steady pace

### Messaging Providers

| Provider | Use Case | Persistence |
|----------|----------|-------------|
| **DevEmulator** | Local dev — PostgreSQL-backed queue simulator | PostgreSQL tables |
| **InMemory** | Unit/integration tests | In-process channels |
| **SQS** | Production AWS — long polling, receive + delete pattern | AWS SQS managed |

**Production AWS Setup:**
- **Services:** ECS Fargate (API + Worker tasks)
- **Queue:** SQS Standard (at-least-once delivery, idempotency handles duplicates)
- **Database:** RDS PostgreSQL with read replicas
- **Queue URL resolution:** `GetQueueUrlAsync` resolves from queue name at startup — no hardcoded URLs
- **Credentials:** Standard AWS credential chain (environment variables, IAM roles, profiles)

---

## Scaling Strategy

### API Service (ECS Fargate)

- **Desired Count:** 2 (minimum for HA)
- **Auto-scaling:** Target CPU 70%, scale to 4 tasks under load
- **Load Balancer:** Application Load Balancer distributes traffic across instances

### Worker Service (ECS Fargate)

- **Desired Count:** 2 (minimum for HA)
- **Auto-scaling:** Based on SQS queue depth
  - Scale to 10 tasks if `ApproximateNumberOfMessagesVisible > 100`
  - Scale down to 2 if queue depth < 10 for 5 minutes
- **Concurrency:** Each task processes one message at a time (idempotency handles retries)
- **Processing:** `SELECT FOR UPDATE SKIP LOCKED` for concurrent message consumption

### Network Architecture

**Public Subnets:** 2 across AZs (Availability Zones)
- CIDR: `10.0.1.0/24`, `10.0.2.0/24`
- Contains: Application Load Balancer / API Gateway

**Private Subnets:** 2 across AZs
- CIDR: `10.0.10.0/24`, `10.0.11.0/24`
- Contains: ECS (Elastic Container Service) tasks, RDS (Relational Database Service)

---

## Monitoring & Observability

### Logging & Middleware

**ErrorHandlingMiddleware**
- Logs detailed errors with full context
- Returns generic messages to client (security best practice)
- Leverage error rate as metric to trigger alerts (errors per minute)
- Watch for specific exceptions (3rd party failures, database issues)

**RequestLoggingMiddleware**
- Logs request start and completion
- Tracks latency per endpoint
- Includes correlation IDs for distributed tracing

**LoggingBehavior (MediatR Pipeline)**
- Logs details about each mediator command/query being handled
- Enables audit trail of all business operations
- Captures execution time and parameters

**Health Checks**
- `/health/live` - Basic service liveness check
- `/health/ready` - Database + worker/messaging readiness checks  
- `/health` - All health checks
- Liveness probes for container orchestration
- Readiness probes for load balancer traffic routing

### Queue Metrics & Alerts

Leverage the queue for metrics to trigger alerts:
- **Message count published per minute** — Ingestion rate
- **Message count consumed per minute** — Processing rate
- **Queue message count per minute** — Queue depth trend
- **Dead letter queue entry count** — Failed message accumulation
- **Queue latency (P95)** — Time from publish to consumption
- **Worker task count** — Auto-scaling activity

---

## Handling Burst Traffic

**SQS Absorption:**
- SQS handles unlimited messages and absorbs burst traffic
- API responds quickly (202 Accepted)
- Workers process at steady pace asynchronously

**Independent Scaling:**
- Separated Command from Query instances for independent scaling
- Command instances: Higher minimum pod count (more traffic expected)
- Query instances: Lower minimum pod count (read-heavy, cacheable)
- Multiple worker replicas process queue messages concurrently

**Immediate Response Pattern:**
- Purchase ingestion happens immediately (synchronous)
- Points calculations and notifications sent to queue (asynchronous)
- Client doesn't wait for entire process to complete
- Improves perceived latency and user experience

**Scaling Metrics:**
Services can scale based on:
- **Caching** — Reduce database queries
- **Database connections** — Connection pooling prevents exhaustion
- **Queue depth** — Messages waiting to be processed
- **CPU usage** — Compute-intensive operations
- **Memory usage** — Data structure allocations
- **Requests per second** — Traffic rate
- **Latency (P95)** — Response time percentile

⚠️ **Critical:** Scale before instances get overloaded and become very slow. Proactive scaling prevents degradation.

---

## Reward Rules & Strategy Pattern

**Extensibility via Strategy Pattern:**
- Reward rules are interchangeable and extensible
- `PointsCalculationService` uses strategy pattern
- Switchable via `appsettings.json` configuration
- Supports multiple implementation approaches:
  - **Compile-time polymorphism** — Different implementations per environment
  - **Runtime polymorphism** — Factory resolves strategy at runtime
  - **Merchant-specific rules** — Different strategies per merchant with effective dates

**Example Strategies:**
- Standard: 1 point per 10 ZAR (floor)
- Tiered: Higher multiplier for VIP customers
- Merchant-specific: Coffee shop = 1.5x points
- Time-based: Double points on weekends

---

## Design Decisions & Trade-offs

| Decision | Benefit | Trade-off |
|----------|---------|-----------|
| **Stateless purchases** | Simpler domain model, easier to reason about | Can't cancel after ingestion |
| **DevEmulator over LocalStack** | More realistic queue behavior, closer to production | Adds PostgreSQL dependency for local dev |
| **MediatR pipeline** | Clean cross-cutting concerns, reusable behaviors | Extra abstraction layer, learning curve |
| **In-memory integration tests** | Fast execution, no external dependencies | Not testing real PostgreSQL behavior |
| **Onion Architecture** | Domain logic is pure and testable, infrastructure is swappable, simpler business abstraction | Less flexibility for testing different use cases |
| **CQRS separation** | Independent scaling of reads/writes, optimized data models | Eventual consistency, more complex deployment |

---

## Design Principles Applied

1. **Encapsulation** — Entity state is private-set; mutations go through domain methods
2. **Abstraction** — Infrastructure concerns behind interfaces (`IPurchaseRepository`, `IMessagePublisher`)
3. **Polymorphism** — `IMessagePublisher` has SQS, InMemory, and DevEmulator implementations
4. **Single Responsibility Principle (SRP)** — Each service has a single responsibility
5. **Open/Closed Principle (OCP)** — Strategy pattern for points calculation; new rules without modifying existing code
6. **Dependency Inversion Principle (DIP)** — Domain defines contracts; Infrastructure provides implementations

---

## Implementation Notes

**Idempotency at Two Levels:**
- **Purchase storage:** `transaction_id` unique constraint prevents duplicate ingestion
- **Ledger entry:** `transaction_id` unique constraint prevents duplicate points awards
- Handles SQS at-least-once delivery semantics

**Correlation IDs:**
- `X-Correlation-Id` header propagated through all layers
- Enables distributed tracing across API, Worker, and database
- Simplifies debugging and monitoring

**Connection Pooling:**
- EF Core connection pooling prevents database exhaustion
- Configurable pool size based on workload
- Reuses connections across requests


