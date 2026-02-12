using Microsoft.EntityFrameworkCore;
using Zapper.LoyaltyPoints.Domain.Entities;
using Zapper.LoyaltyPoints.Domain.Entities.Queue;
using Zapper.LoyaltyPoints.Domain.Interfaces;

namespace Zapper.LoyaltyPoints.Infrastructure.Persistence;

public class LoyaltyDbContext : DbContext, IUnitOfWork
{
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PointsLedgerEntry> PointsLedgerEntries => Set<PointsLedgerEntry>();
    public DbSet<CustomerBalance> CustomerBalances => Set<CustomerBalance>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<QueueMessage> QueueMessages => Set<QueueMessage>();
    public DbSet<QueueHistory> QueueHistory => Set<QueueHistory>();
    public DbSet<FailedMessage> FailedMessages => Set<FailedMessage>();

    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.ToTable("merchants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MerchantCode).HasColumnName("merchant_code").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(320);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.MerchantCode).IsUnique().HasDatabaseName("ix_merchants_code");
        });
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerCode).HasColumnName("customer_code").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.CustomerCode).IsUnique().HasDatabaseName("ix_customers_code");
        });
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("purchases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.MerchantId).HasColumnName("merchant_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
            entity.Property(e => e.TransactionTimestamp).HasColumnName("transaction_timestamp").IsRequired();
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PointsAwarded).HasColumnName("points_awarded").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.TransactionId).IsUnique().HasDatabaseName("ix_purchases_transaction_id");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("ix_purchases_customer_id");
            entity.HasIndex(e => new { e.CustomerId, e.MerchantId }).HasDatabaseName("ix_purchases_customer_merchant");
        });
        modelBuilder.Entity<PointsLedgerEntry>(entity =>
        {
            entity.ToTable("points_ledger_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.MerchantId).HasColumnName("merchant_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Points).HasColumnName("points").IsRequired();
            entity.Property(e => e.PurchaseAmount).HasColumnName("purchase_amount").HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.TransactionId).IsUnique().HasDatabaseName("ix_ledger_transaction_id");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("ix_ledger_customer_id");
            entity.HasIndex(e => new { e.CustomerId, e.MerchantId }).HasDatabaseName("ix_ledger_customer_merchant");
        });
        modelBuilder.Entity<CustomerBalance>(entity =>
        {
            entity.ToTable("customer_balances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.MerchantId).HasColumnName("merchant_id").HasMaxLength(256).IsRequired();
            entity.Property(e => e.TotalPoints).HasColumnName("total_points").IsRequired();
            entity.Property(e => e.Version).HasColumnName("version").IsConcurrencyToken();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.CustomerId, e.MerchantId })
                .IsUnique()
                .HasDatabaseName("ix_balances_customer_merchant");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("ix_balances_customer_id");
        });
        modelBuilder.Entity<QueueMessage>(entity =>
        {
            entity.ToTable("queue_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries").IsRequired();
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
            entity.Property(e => e.LockedUntil).HasColumnName("locked_until");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ErrorStackTrace).HasColumnName("error_stack_trace");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_queue_messages_status");
            entity.HasIndex(e => e.ScheduledAt).HasDatabaseName("ix_queue_messages_scheduled_at");
        });
        modelBuilder.Entity<QueueHistory>(entity =>
        {
            entity.ToTable("queue_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QueueMessageId).HasColumnName("queue_message_id").IsRequired();
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ElapsedMs).HasColumnName("elapsed_ms");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.AttemptNumber).HasColumnName("attempt_number");
            entity.HasIndex(e => e.QueueMessageId).HasDatabaseName("ix_queue_history_queue_message_id");
        });
        modelBuilder.Entity<FailedMessage>(entity =>
        {
            entity.ToTable("failed_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OriginalMessageId).HasColumnName("original_message_id").IsRequired();
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(256).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").IsRequired();
            entity.Property(e => e.LastErrorMessage).HasColumnName("last_error_message").HasMaxLength(1000);
            entity.Property(e => e.LastStackTrace).HasColumnName("last_stack_trace");
            entity.Property(e => e.TotalAttempts).HasColumnName("total_attempts").IsRequired();
            entity.Property(e => e.CanRetry).HasColumnName("can_retry").IsRequired();
            entity.Property(e => e.FailedAt).HasColumnName("failed_at");
            entity.Property(e => e.RetriedAt).HasColumnName("retried_at");
            entity.HasIndex(e => e.OriginalMessageId).HasDatabaseName("ix_failed_messages_original_message_id");
        });
    }
}
