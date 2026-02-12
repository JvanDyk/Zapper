using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapper.LoyaltyPoints.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    total_points = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_balances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "failed_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    last_error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_stack_trace = table.Column<string>(type: "text", nullable: true),
                    total_attempts = table.Column<int>(type: "integer", nullable: false),
                    can_retry = table.Column<bool>(type: "boolean", nullable: false),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    retried_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_failed_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "points_ledger_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    purchase_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_ledger_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    customer_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    transaction_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    points_awarded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "queue_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    elapsed_ms = table.Column<long>(type: "bigint", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    attempt_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "queue_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_retries = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    error_stack_trace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_balances_customer_id",
                table: "customer_balances",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_balances_customer_merchant",
                table: "customer_balances",
                columns: new[] { "customer_id", "merchant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_code",
                table: "customers",
                column: "customer_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_failed_messages_original_message_id",
                table: "failed_messages",
                column: "original_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_code",
                table: "merchants",
                column: "merchant_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ledger_customer_id",
                table: "points_ledger_entries",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_customer_merchant",
                table: "points_ledger_entries",
                columns: new[] { "customer_id", "merchant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transaction_id",
                table: "points_ledger_entries",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_customer_id",
                table: "purchases",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_customer_merchant",
                table: "purchases",
                columns: new[] { "customer_id", "merchant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchases_transaction_id",
                table: "purchases",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_queue_history_queue_message_id",
                table: "queue_history",
                column: "queue_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_queue_messages_scheduled_at",
                table: "queue_messages",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "ix_queue_messages_status",
                table: "queue_messages",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_balances");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "failed_messages");

            migrationBuilder.DropTable(
                name: "merchants");

            migrationBuilder.DropTable(
                name: "points_ledger_entries");

            migrationBuilder.DropTable(
                name: "purchases");

            migrationBuilder.DropTable(
                name: "queue_history");

            migrationBuilder.DropTable(
                name: "queue_messages");
        }
    }
}
