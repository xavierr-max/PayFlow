using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStripeWithAsaas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PremiumSubscriptions_StripeCheckoutSessionId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_PremiumSubscriptions_StripeSubscriptionId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripeChargeId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripeCheckoutSessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ExternalPaymentIntentId",
                table: "PremiumSubscriptionPayments",
                newName: "ExternalProviderPaymentId");

            migrationBuilder.RenameColumn(
                name: "ExternalPaymentIntentId",
                table: "PaymentTransactions",
                newName: "ExternalProviderPaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_ExternalPaymentIntentId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_ExternalProviderPaymentId");

            migrationBuilder.DropColumn(
                name: "ConnectedAccountId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "StripeCheckoutSessionId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "StripePriceId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "PremiumSubscriptions");

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
                name: "StripeCheckoutSessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeCheckoutUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeCheckoutUrlExpiresAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeConnectedAccountId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentMethod",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentStatus",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasAccountId",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasApiKey",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasSubaccountStatus",
                table: "Sellers",
                type: "text",
                nullable: false,
                defaultValue: "pending_data");

            migrationBuilder.AddColumn<string>(
                name: "AsaasWalletId",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfCnpj",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IncomeValue",
                table: "Sellers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCheckoutUrl",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderCustomerId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderSubscriptionId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasBillingType",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasCustomerId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasInvoiceUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasPaymentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasPaymentStatus",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasCustomerId",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfCnpj",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_ProviderCustomerId",
                table: "PremiumSubscriptions",
                column: "ProviderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_ProviderSubscriptionId",
                table: "PremiumSubscriptions",
                column: "ProviderSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AsaasPaymentId",
                table: "Payments",
                column: "AsaasPaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PremiumSubscriptions_ProviderCustomerId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_PremiumSubscriptions_ProviderSubscriptionId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AsaasPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AsaasAccountId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AsaasApiKey",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AsaasSubaccountStatus",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AsaasWalletId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "CpfCnpj",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "IncomeValue",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "ProviderCheckoutUrl",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderCustomerId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderSubscriptionId",
                table: "PremiumSubscriptions");

            migrationBuilder.DropColumn(
                name: "AsaasBillingType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AsaasCustomerId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AsaasInvoiceUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AsaasPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AsaasPaymentStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AsaasCustomerId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CpfCnpj",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "ExternalProviderPaymentId",
                table: "PremiumSubscriptionPayments",
                newName: "ExternalPaymentIntentId");

            migrationBuilder.RenameColumn(
                name: "ExternalProviderPaymentId",
                table: "PaymentTransactions",
                newName: "ExternalPaymentIntentId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_ExternalProviderPaymentId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_ExternalPaymentIntentId");

            migrationBuilder.AddColumn<string>(
                name: "ConnectedAccountId",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCheckoutSessionId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePriceId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "PremiumSubscriptions",
                type: "text",
                nullable: true);

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
                name: "StripeCheckoutSessionId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCheckoutUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StripeCheckoutUrlExpiresAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectedAccountId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentMethod",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentStatus",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_StripeCheckoutSessionId",
                table: "PremiumSubscriptions",
                column: "StripeCheckoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumSubscriptions_StripeSubscriptionId",
                table: "PremiumSubscriptions",
                column: "StripeSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripeChargeId",
                table: "Payments",
                column: "StripeChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripeCheckoutSessionId",
                table: "Payments",
                column: "StripeCheckoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId");
        }
    }
}
