using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndStaffCounterSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.StaffUsers') AND name = 'Email'
                      AND max_length = -1
                )
                BEGIN
                    ALTER TABLE [dbo].[StaffUsers] ALTER COLUMN [Email] nvarchar(200) NOT NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.StaffCounterSessions', 'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[StaffCounterSessions] (
                        [Id] int NOT NULL IDENTITY,
                        [StaffUserId] int NOT NULL,
                        [CounterId] int NOT NULL,
                        [StartedAt] datetime2 NOT NULL,
                        [EndedAt] datetime2 NULL,
                        [Status] nvarchar(20) NOT NULL,
                        [LoginIp] nvarchar(64) NULL,
                        [DeviceName] nvarchar(200) NULL,
                        [Remarks] nvarchar(500) NULL,
                        CONSTRAINT [PK_StaffCounterSessions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_StaffCounterSessions_Counters_CounterId] FOREIGN KEY ([CounterId]) REFERENCES [Counters] ([Id]),
                        CONSTRAINT [FK_StaffCounterSessions_StaffUsers_StaffUserId] FOREIGN KEY ([StaffUserId]) REFERENCES [StaffUsers] ([Id])
                    );
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_Email' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                    CREATE UNIQUE INDEX [IX_StaffUsers_Email] ON [dbo].[StaffUsers] ([Email]);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffCounterSessions_CounterId_Status' AND object_id = OBJECT_ID('dbo.StaffCounterSessions'))
                    CREATE INDEX [IX_StaffCounterSessions_CounterId_Status] ON [dbo].[StaffCounterSessions] ([CounterId], [Status]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffCounterSessions_EndedAt' AND object_id = OBJECT_ID('dbo.StaffCounterSessions'))
                    CREATE INDEX [IX_StaffCounterSessions_EndedAt] ON [dbo].[StaffCounterSessions] ([EndedAt]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffCounterSessions_StaffUserId_Status' AND object_id = OBJECT_ID('dbo.StaffCounterSessions'))
                    CREATE INDEX [IX_StaffCounterSessions_StaffUserId_Status] ON [dbo].[StaffCounterSessions] ([StaffUserId], [Status]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffCounterSessions_StartedAt' AND object_id = OBJECT_ID('dbo.StaffCounterSessions'))
                    CREATE INDEX [IX_StaffCounterSessions_StartedAt] ON [dbo].[StaffCounterSessions] ([StartedAt]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.StaffCounterSessions', 'U') IS NOT NULL
                    DROP TABLE [dbo].[StaffCounterSessions];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_Email' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                    DROP INDEX [IX_StaffUsers_Email] ON [dbo].[StaffUsers];
                """);
        }
    }
}
