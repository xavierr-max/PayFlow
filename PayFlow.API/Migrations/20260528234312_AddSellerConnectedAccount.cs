using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerConnectedAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectedAccountId",
                table: "Sellers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedAccountId",
                table: "Sellers");
        }
    }
}
