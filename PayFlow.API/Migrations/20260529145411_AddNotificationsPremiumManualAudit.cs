using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsPremiumManualAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorEmail",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActorUserId",
                table: "PaymentTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualPaidAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualPaidByUserId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManualPaidPreviousStatus",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualPaidReason",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualPaymentReversalReason",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManualPaymentReversedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualPaymentReversedByUserId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    TargetUrl = table.Column<string>(type: "text", nullable: true),
                    DeduplicationKey = table.Column<string>(type: "text", nullable: true),
                    Channels = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PremiumSubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PremiumSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ExternalInvoiceId = table.Column<string>(type: "text", nullable: true),
                    ExternalPaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    ExternalChargeId = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumSubscriptionPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PremiumSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    StripeCheckoutSessionId = table.Column<string>(type: "text", nullable: true),
                    StripePriceId = table.Column<string>(type: "text", nullable: true),
                    CancelAtPeriodEnd = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeduplicationKey",
                table: "Notifications",
                column: "DeduplicationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SellerId",
                table: "Notifications",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SellerId_IsRead",
                table: "Notifications",
                columns: new[] { "SellerId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptionPayments_ExternalInvoiceId",
                table: "PremiumSubscriptionPayments",
                column: "ExternalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptionPayments_PremiumSubscriptionId",
                table: "PremiumSubscriptionPayments",
                column: "PremiumSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_SellerId",
                table: "PremiumSubscriptions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_StripeCheckoutSessionId",
                table: "PremiumSubscriptions",
                column: "StripeCheckoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_StripeSubscriptionId",
                table: "PremiumSubscriptions",
                column: "StripeSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_UserId",
                table: "PremiumSubscriptions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PremiumSubscriptionPayments");

            migrationBuilder.DropTable(
                name: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "ActorEmail",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ManualPaidAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaidByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaidPreviousStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaidReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaymentReversalReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaymentReversedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ManualPaymentReversedByUserId",
                table: "Payments");
        }
    }
}
