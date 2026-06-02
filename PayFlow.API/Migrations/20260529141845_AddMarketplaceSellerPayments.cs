using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceSellerPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardInstallmentCount",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardLast4",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "Payments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Payments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Payments" AS p
                SET "SellerId" = c."SellerId"
                FROM "Customers" AS c
                WHERE p."CustomerId" = c."Id";
                """);

            migrationBuilder.Sql("""
                UPDATE "Payments"
                SET "SellerId" = '00000000-0000-0000-0000-000000000000'
                WHERE "SellerId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "SellerId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StripeApplicationFeeAmount",
                table: "Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeChargeId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeChargeType",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectedAccountId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    InstallmentCount = table.Column<int>(type: "integer", nullable: true),
                    ProviderAccountId = table.Column<string>(type: "text", nullable: true),
                    ProviderEventId = table.Column<string>(type: "text", nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "text", nullable: true),
                    ExternalPaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    ExternalChargeId = table.Column<string>(type: "text", nullable: true),
                    ExternalRefundId = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    RawPayload = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEventLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SellerId",
                table: "Payments",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripeCheckoutSessionId",
                table: "Payments",
                column: "StripeCheckoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripeChargeId",
                table: "Payments",
                column: "StripeChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ExternalChargeId",
                table: "PaymentTransactions",
                column: "ExternalChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ExternalPaymentIntentId",
                table: "PaymentTransactions",
                column: "ExternalPaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SellerId",
                table: "PaymentTransactions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEventLogs_Provider_EventId",
                table: "WebhookEventLogs",
                columns: new[] { "Provider", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "WebhookEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SellerId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripeCheckoutSessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripeChargeId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardInstallmentCount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardLast4",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeApplicationFeeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeChargeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeChargeType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeConnectedAccountId",
                table: "Payments");
        }
    }
}
