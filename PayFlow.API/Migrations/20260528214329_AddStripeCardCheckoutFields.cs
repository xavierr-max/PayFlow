using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeCardCheckoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentMessageSentAt",
                table: "Payments",
                type: "timestamp with time zone",
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
                name: "StripePaymentMethod",
                table: "Payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMessageSentAt",
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
                name: "StripePaymentMethod",
                table: "Payments");
        }
    }
}
