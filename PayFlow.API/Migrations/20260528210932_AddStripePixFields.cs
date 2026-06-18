using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStripePixFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PixCopyPaste",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PixExpiresAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixHostedInstructionsUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PixMessageSentAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixQrCodeImageUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixQrCodeSvgUrl",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentStatus",
                table: "Payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PixCopyPaste",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PixExpiresAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PixHostedInstructionsUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PixMessageSentAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PixQrCodeImageUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PixQrCodeSvgUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentStatus",
                table: "Payments");
        }
    }
}
