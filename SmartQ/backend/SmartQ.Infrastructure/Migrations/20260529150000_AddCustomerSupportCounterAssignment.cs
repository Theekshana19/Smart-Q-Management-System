using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQ.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCustomerSupportCounterAssignment : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "CounterServiceAssignments",
            columns: new[] { "Id", "CounterId", "ServiceId", "IsActive" },
            values: new object[] { 6, 1, 5, true });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "CounterServiceAssignments",
            keyColumn: "Id",
            keyValue: 6);
    }
}
