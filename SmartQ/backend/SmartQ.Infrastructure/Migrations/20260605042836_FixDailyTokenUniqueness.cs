using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDailyTokenUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Tokens_TokenNo'
                      AND object_id = OBJECT_ID('Tokens')
                      AND is_unique = 1)
                BEGIN
                    DROP INDEX [IX_Tokens_TokenNo] ON [Tokens];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Tokens', 'SequenceDate') IS NULL
                BEGIN
                    ALTER TABLE [Tokens] ADD [SequenceDate] date NOT NULL CONSTRAINT DF_Tokens_SequenceDate DEFAULT '0001-01-01';
                END
                """);

            migrationBuilder.Sql("""
                UPDATE Tokens
                SET SequenceDate = CAST(CreatedAt AS date)
                WHERE SequenceDate = '0001-01-01';
                """);

            migrationBuilder.Sql("""
                ;WITH ranked AS (
                    SELECT
                        Id,
                        TokenPrefix,
                        ROW_NUMBER() OVER (
                            PARTITION BY SubServiceId, SequenceDate
                            ORDER BY CreatedAt, Id) AS NewSeq
                    FROM Tokens
                )
                UPDATE t
                SET
                    SequenceNo = r.NewSeq,
                    TokenNo = r.TokenPrefix + '-' + RIGHT('000' + CAST(r.NewSeq AS varchar(3)), 3)
                FROM Tokens t
                INNER JOIN ranked r ON t.Id = r.Id;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Tokens_SubServiceId_SequenceDate_SequenceNo'
                      AND object_id = OBJECT_ID('Tokens'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_Tokens_SubServiceId_SequenceDate_SequenceNo]
                    ON [Tokens] ([SubServiceId], [SequenceDate], [SequenceNo]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Tokens_TokenNo'
                      AND object_id = OBJECT_ID('Tokens'))
                BEGIN
                    CREATE INDEX [IX_Tokens_TokenNo] ON [Tokens] ([TokenNo]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_SubServiceId_SequenceDate_SequenceNo",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_TokenNo",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "SequenceDate",
                table: "Tokens");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenNo",
                table: "Tokens",
                column: "TokenNo",
                unique: true);
        }
    }
}
