# Zapper Loyalty Points Service

A loyalty points microservice built with **C# (.NET 10)**, **PostgreSQL**, and **AWS SQS**. Accepts purchase events, calculates loyalty points, and maintains customer balances.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- PostgreSQL (or use Docker)

## Installation


### Database Setup

Start PostgreSQL:
```bash
docker run -d --name loyalty-pg \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=loyalty_points \
  -p 5432:5432 \
  postgres:16-alpine
```

Migrations run automatically on first API startup.

## Running Locally

### With dotnet CLI

**Terminal 1 — Command API:**
```bash
dotnet run --project src/Zapper.LoyaltyPoints.Api.Commands --launch-profile http
```
- API: http://localhost:5000
- Docs: http://localhost:5000/scalar/v1

**Terminal 2 — Query API:**
```bash
dotnet run --project src/Zapper.LoyaltyPoints.Api.Queries --launch-profile http
```
- API: http://localhost:5002
- Docs: http://localhost:5002/scalar/v1

**Terminal 3 — Worker:**
```bash
dotnet run --project src/Zapper.LoyaltyPoints.Worker
```

### With Visual Studio

1. Set multiple startup projects:
   - `Zapper.LoyaltyPoints.Api.Commands`
   - `Zapper.LoyaltyPoints.Api.Queries`
   - `Zapper.LoyaltyPoints.Worker`
2. Press F5 to start all three

## Docker

### Build & Run

```bash
cd docker
docker-compose up --build
```

Services:
- **postgres** — port 5432
- **command-api** — port 5000
- **query-api** — port 5002
- **worker** — 2 replicas

### Stop Services

```bash
docker-compose down
```

### View Logs

```bash
docker-compose logs -f command-api
docker-compose logs -f query-api
docker-compose logs -f worker
```

## Testing

```bash
dotnet test
```

## Documentation

See [SOLUTION.md](SOLUTION.md) for:
- Architecture & design patterns
- Data model & schema
- Configuration options
- API endpoints & examples
- Demo data
