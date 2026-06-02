using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffConsoleIndexesAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tokens_CalledAt' AND object_id = OBJECT_ID('dbo.Tokens'))
                    CREATE INDEX [IX_Tokens_CalledAt] ON [dbo].[Tokens] ([CalledAt]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tokens_CompletedAt' AND object_id = OBJECT_ID('dbo.Tokens'))
                    CREATE INDEX [IX_Tokens_CompletedAt] ON [dbo].[Tokens] ([CompletedAt]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TokenStatusHistories_ChangedAt' AND object_id = OBJECT_ID('dbo.TokenStatusHistories'))
                    CREATE INDEX [IX_TokenStatusHistories_ChangedAt] ON [dbo].[TokenStatusHistories] ([ChangedAt]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_CounterId' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                    CREATE INDEX [IX_StaffUsers_CounterId] ON [dbo].[StaffUsers] ([CounterId]);
            """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_Username' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns c
                        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                        WHERE c.object_id = OBJECT_ID('dbo.StaffUsers')
                          AND c.name = 'Username'
                          AND c.max_length <> -1
                          AND t.name NOT IN ('text', 'ntext', 'image', 'xml')
                    )
                    BEGIN
                        IF NOT EXISTS (
                            SELECT [Username]
                            FROM [dbo].[StaffUsers]
                            WHERE [Username] IS NOT NULL
                            GROUP BY [Username]
                            HAVING COUNT(1) > 1
                        )
                            CREATE UNIQUE INDEX [IX_StaffUsers_Username] ON [dbo].[StaffUsers] ([Username]);
                        ELSE
                            CREATE INDEX [IX_StaffUsers_Username] ON [dbo].[StaffUsers] ([Username]);
                    END
                END
            """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'WAIT_TIME_WARNING_MINUTES')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('WAIT_TIME_WARNING_MINUTES', '10', 'int', 'Waiting threshold warning in minutes', 1);

                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'STAFF_UI_THEME')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('STAFF_UI_THEME', 'ENTERPRISE_TEAL', 'string', 'Staff UI theme', 1);

                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'TRANSFER_REGENERATE_TOKEN')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('TRANSFER_REGENERATE_TOKEN', 'false', 'bool', 'Regenerate token number on transfer', 1);

                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'STAFF_AUTO_REFRESH_SECONDS')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('STAFF_AUTO_REFRESH_SECONDS', '10', 'int', 'Staff console auto refresh interval', 1);

                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN', 'true', 'bool', 'Lock call next while active token exists', 1);

                IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SYSTEM_ONLINE')
                INSERT INTO SystemSettings (SettingKey, SettingValue, DataType, Description, IsActive)
                VALUES ('SYSTEM_ONLINE', 'true', 'bool', 'System online/offline flag', 1);
            """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_EMPTY_QUEUE_TITLE')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_EMPTY_QUEUE_TITLE', 'No active token', 1, 90);

                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_EMPTY_QUEUE_DESCRIPTION')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_EMPTY_QUEUE_DESCRIPTION', 'The counter is currently idle. Click Call Next to serve the next customer in the priority queue.', 1, 91);

                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_CALL_NEXT_LOCKED_MESSAGE')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_CALL_NEXT_LOCKED_MESSAGE', 'Complete or skip current token before calling next', 1, 92);

                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_TRANSFER_WARNING')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_TRANSFER_WARNING', 'Transferred token will leave this counter''s queue and be added to the destination''s waiting list immediately.', 1, 93);

                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_COUNTER_SERVES_MESSAGE')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_COUNTER_SERVES_MESSAGE', 'You are currently handling assigned services for this counter.', 1, 94);

                IF NOT EXISTS (SELECT 1 FROM DisplayMessages WHERE MessageKey = 'STAFF_TV_BRANCH_MESSAGE')
                INSERT INTO DisplayMessages (LanguageId, MessageKey, MessageText, IsActive, DisplayOrder)
                VALUES (NULL, 'STAFF_TV_BRANCH_MESSAGE', 'Public display shows branch-wide waiting and called tokens.', 1, 95);
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tokens_CalledAt' AND object_id = OBJECT_ID('dbo.Tokens'))
                    DROP INDEX [IX_Tokens_CalledAt] ON [dbo].[Tokens];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tokens_CompletedAt' AND object_id = OBJECT_ID('dbo.Tokens'))
                    DROP INDEX [IX_Tokens_CompletedAt] ON [dbo].[Tokens];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TokenStatusHistories_ChangedAt' AND object_id = OBJECT_ID('dbo.TokenStatusHistories'))
                    DROP INDEX [IX_TokenStatusHistories_ChangedAt] ON [dbo].[TokenStatusHistories];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_CounterId' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                    DROP INDEX [IX_StaffUsers_CounterId] ON [dbo].[StaffUsers];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffUsers_Username' AND object_id = OBJECT_ID('dbo.StaffUsers'))
                    DROP INDEX [IX_StaffUsers_Username] ON [dbo].[StaffUsers];
            """);

            migrationBuilder.Sql("DELETE FROM SystemSettings WHERE SettingKey IN ('WAIT_TIME_WARNING_MINUTES','STAFF_UI_THEME','TRANSFER_REGENERATE_TOKEN','STAFF_AUTO_REFRESH_SECONDS','CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN','SYSTEM_ONLINE');");
            migrationBuilder.Sql("DELETE FROM DisplayMessages WHERE MessageKey IN ('STAFF_EMPTY_QUEUE_TITLE','STAFF_EMPTY_QUEUE_DESCRIPTION','STAFF_CALL_NEXT_LOCKED_MESSAGE','STAFF_TRANSFER_WARNING','STAFF_COUNTER_SERVES_MESSAGE','STAFF_TV_BRANCH_MESSAGE');");
        }
    }
}
