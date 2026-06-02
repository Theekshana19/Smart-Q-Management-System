using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenTransferTrackingAndQueuedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTransferredAt",
                table: "Tokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QueuedAt",
                table: "Tokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferCount",
                table: "Tokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransferredFromTokenNo",
                table: "Tokens",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("UPDATE [Tokens] SET [QueuedAt] = [CreatedAt] WHERE [QueuedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_Status_QueuedAt",
                table: "Tokens",
                columns: new[] { "Status", "QueuedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_Status_QueuedAt",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "LastTransferredAt",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "QueuedAt",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "TransferCount",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "TransferredFromTokenNo",
                table: "Tokens");
        }
    }
}
