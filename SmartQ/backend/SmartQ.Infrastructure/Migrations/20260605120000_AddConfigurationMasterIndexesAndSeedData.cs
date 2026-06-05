using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQ.Infrastructure.Migrations
{
    public class AddConfigurationMasterIndexesAndSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Languages') AND name = 'Code' AND max_length = -1)
                    ALTER TABLE [dbo].[Languages] ALTER COLUMN [Code] nvarchar(10) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Languages') AND name = 'Name' AND max_length = -1)
                    ALTER TABLE [dbo].[Languages] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Languages') AND name = 'NativeName' AND max_length = -1)
                    ALTER TABLE [dbo].[Languages] ALTER COLUMN [NativeName] nvarchar(100) NOT NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SystemSettings') AND name = 'SettingKey' AND max_length = -1)
                    ALTER TABLE [dbo].[SystemSettings] ALTER COLUMN [SettingKey] nvarchar(100) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SystemSettings') AND name = 'SettingValue' AND max_length = -1)
                    ALTER TABLE [dbo].[SystemSettings] ALTER COLUMN [SettingValue] nvarchar(2000) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SystemSettings') AND name = 'DataType' AND max_length = -1)
                    ALTER TABLE [dbo].[SystemSettings] ALTER COLUMN [DataType] nvarchar(20) NOT NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DisplayMessages') AND name = 'MessageKey' AND max_length = -1)
                    ALTER TABLE [dbo].[DisplayMessages] ALTER COLUMN [MessageKey] nvarchar(100) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DisplayMessages') AND name = 'MessageText' AND max_length = -1)
                    ALTER TABLE [dbo].[DisplayMessages] ALTER COLUMN [MessageText] nvarchar(2000) NOT NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VoiceTemplates') AND name = 'EventType' AND max_length = -1)
                    ALTER TABLE [dbo].[VoiceTemplates] ALTER COLUMN [EventType] nvarchar(50) NOT NULL;
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VoiceTemplates') AND name = 'TemplateText' AND max_length = -1)
                    ALTER TABLE [dbo].[VoiceTemplates] ALTER COLUMN [TemplateText] nvarchar(1000) NOT NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Languages_Code' AND object_id = OBJECT_ID('dbo.Languages'))
                    CREATE UNIQUE INDEX [IX_Languages_Code] ON [dbo].[Languages] ([Code]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Languages_IsActive_DisplayOrder' AND object_id = OBJECT_ID('dbo.Languages'))
                    CREATE INDEX [IX_Languages_IsActive_DisplayOrder] ON [dbo].[Languages] ([IsActive], [DisplayOrder]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SystemSettings_SettingKey' AND object_id = OBJECT_ID('dbo.SystemSettings'))
                    CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [dbo].[SystemSettings] ([SettingKey]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_MessageKey_LanguageId' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    CREATE UNIQUE INDEX [IX_DisplayMessages_MessageKey_LanguageId] ON [dbo].[DisplayMessages] ([MessageKey], [LanguageId]) WHERE [LanguageId] IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_LanguageId_IsActive' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    CREATE INDEX [IX_DisplayMessages_LanguageId_IsActive] ON [dbo].[DisplayMessages] ([LanguageId], [IsActive]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_MessageKey' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    CREATE INDEX [IX_DisplayMessages_MessageKey] ON [dbo].[DisplayMessages] ([MessageKey]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VoiceTemplates_LanguageId_EventType' AND object_id = OBJECT_ID('dbo.VoiceTemplates'))
                    CREATE UNIQUE INDEX [IX_VoiceTemplates_LanguageId_EventType] ON [dbo].[VoiceTemplates] ([LanguageId], [EventType]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VoiceTemplates_LanguageId_IsActive' AND object_id = OBJECT_ID('dbo.VoiceTemplates'))
                    CREATE INDEX [IX_VoiceTemplates_LanguageId_IsActive] ON [dbo].[VoiceTemplates] ([LanguageId], [IsActive]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VoiceTemplates_LanguageId_IsActive' AND object_id = OBJECT_ID('dbo.VoiceTemplates'))
                    DROP INDEX [IX_VoiceTemplates_LanguageId_IsActive] ON [dbo].[VoiceTemplates];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VoiceTemplates_LanguageId_EventType' AND object_id = OBJECT_ID('dbo.VoiceTemplates'))
                    DROP INDEX [IX_VoiceTemplates_LanguageId_EventType] ON [dbo].[VoiceTemplates];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_MessageKey' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    DROP INDEX [IX_DisplayMessages_MessageKey] ON [dbo].[DisplayMessages];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_LanguageId_IsActive' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    DROP INDEX [IX_DisplayMessages_LanguageId_IsActive] ON [dbo].[DisplayMessages];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisplayMessages_MessageKey_LanguageId' AND object_id = OBJECT_ID('dbo.DisplayMessages'))
                    DROP INDEX [IX_DisplayMessages_MessageKey_LanguageId] ON [dbo].[DisplayMessages];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SystemSettings_SettingKey' AND object_id = OBJECT_ID('dbo.SystemSettings'))
                    DROP INDEX [IX_SystemSettings_SettingKey] ON [dbo].[SystemSettings];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Languages_IsActive_DisplayOrder' AND object_id = OBJECT_ID('dbo.Languages'))
                    DROP INDEX [IX_Languages_IsActive_DisplayOrder] ON [dbo].[Languages];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Languages_Code' AND object_id = OBJECT_ID('dbo.Languages'))
                    DROP INDEX [IX_Languages_Code] ON [dbo].[Languages];
                """);
        }
    }
}
