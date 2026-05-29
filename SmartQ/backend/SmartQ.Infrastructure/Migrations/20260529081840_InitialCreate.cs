using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CounterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffUsers_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DisplayMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    MessageKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayMessages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoiceTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceTemplates_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterServiceAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterServiceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterServiceAssignments_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterServiceAssignments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceTranslations_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedServiceMinutes = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyTokenSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SequenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubServiceId = table.Column<int>(type: "int", nullable: false),
                    TokenPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTokenSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyTokenSequences_SubServices_SubServiceId",
                        column: x => x.SubServiceId,
                        principalTable: "SubServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubServiceTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubServiceId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubServiceTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubServiceTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubServiceTranslations_SubServices_SubServiceId",
                        column: x => x.SubServiceId,
                        principalTable: "SubServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TokenPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    SubServiceId = table.Column<int>(type: "int", nullable: false),
                    CounterId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SkippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedWaitMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tokens_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tokens_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tokens_SubServices_SubServiceId",
                        column: x => x.SubServiceId,
                        principalTable: "SubServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    CounterId = table.Column<int>(type: "int", nullable: true),
                    StaffUserId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenStatusHistories_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Counters",
                columns: new[] { "Id", "CounterName", "CounterNo", "CreatedAt", "IsActive", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Counter 01", "01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), true, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 2, "Counter 02", "02", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), true, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 3, "Counter 03", "03", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), true, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 4, "Counter 04", "04", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), true, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 5, "Counter 05", "05", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), true, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.InsertData(
                table: "DisplayMessages",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "LanguageId", "MessageKey", "MessageText" },
                values: new object[,]
                {
                    { 1, 1, true, null, "TICKER_1", "PLEASE PROCEED TO YOUR COUNTER WHEN YOUR TOKEN IS CALLED" },
                    { 2, 2, true, null, "TICKER_2", "DOWNLOAD THE SMARTQ APP FOR INSTANT NOTIFICATIONS" },
                    { 3, 3, true, null, "TICKER_3", "THANK YOU FOR PATIENTLY WAITING AT SMARTQ BANK" },
                    { 4, 4, true, null, "MOBILE_BANKING", "Scan QR on your receipt to track your turn live." }
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "CreatedAt", "DisplayOrder", "IsActive", "IsDefault", "Name", "NativeName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "EN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, true, true, "English", "English", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 2, "SI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, true, false, "Sinhala", "සිංහල", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 3, "TA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), 3, true, false, "Tamil", "தமிழ்", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "DisplayOrder", "Icon", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "CASH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Deposits, withdrawals, and currency exchange.", 1, "payments", true, "Cash Services", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 2, "ACC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Open new accounts, update profiles, and statement requests.", 2, "account_balance", true, "Account Services", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 3, "LOAN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Mortgages, personal loans, and credit consultations.", 3, "description", true, "Loan Services", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 4, "CARD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "New debit/credit cards, PIN resets, and card blockage.", 4, "credit_card", true, "Card Services", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 5, "HELP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "General inquiries, complaints, and digital banking assistance.", 5, "support_agent", true, "Customer Support", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.InsertData(
                table: "StaffUsers",
                columns: new[] { "Id", "CounterId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "admin@smartq.bank", "Admin User", true, "AQAAAAIAAYagAAAAE", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "admin" });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "DataType", "Description", "IsActive", "SettingKey", "SettingValue" },
                values: new object[,]
                {
                    { 1, "string", "Daily token sequence reset mode", true, "TOKEN_RESET_MODE", "DAILY" },
                    { 2, "bool", "Enable voice announcements on display", true, "ENABLE_VOICE_ANNOUNCEMENT", "true" },
                    { 3, "bool", "Enable token printing on kiosk", true, "ENABLE_PRINT_TOKEN", "true" },
                    { 4, "int", "Number of recently called tokens on display", true, "DISPLAY_RECENT_CALL_COUNT", "3" },
                    { 5, "int", "Number of waiting tokens on display", true, "DISPLAY_WAITING_QUEUE_COUNT", "5" },
                    { 6, "int", "Default estimated wait minutes", true, "DEFAULT_ESTIMATED_WAIT_MINUTES", "8" },
                    { 7, "int", "Kiosk auto return seconds after finish", true, "KIOSK_AUTO_RETURN_SECONDS", "5" },
                    { 8, "bool", "Priority tokens called before standard", true, "ENABLE_PRIORITY_QUEUE", "true" },
                    { 9, "string", "Branch identifier", true, "BRANCH_ID", "BR-9904" },
                    { 10, "string", "Branch display name", true, "BRANCH_NAME", "SmartQ Bank Central Branch" },
                    { 11, "string", "Kiosk software version", true, "KIOSK_VERSION", "v2.4" }
                });

            migrationBuilder.InsertData(
                table: "CounterServiceAssignments",
                columns: new[] { "Id", "CounterId", "IsActive", "ServiceId" },
                values: new object[,]
                {
                    { 1, 1, true, 2 },
                    { 2, 2, true, 1 },
                    { 3, 3, true, 1 },
                    { 4, 4, true, 4 },
                    { 5, 5, true, 3 }
                });

            migrationBuilder.InsertData(
                table: "ServiceTranslations",
                columns: new[] { "Id", "Description", "LanguageId", "Name", "ServiceId" },
                values: new object[,]
                {
                    { 1, "තැන්පතු, නිකුත් කිරීම් සහ මුදල් හුවමාරු.", 2, "මුදල් සේවා", 1 },
                    { 2, "வைப்புகள், திரும்பப் பெறுதல்கள் மற்றும் நாணய பரிமாற்றம்.", 3, "பண சேவைகள்", 1 },
                    { 3, "නව ගිණුම් විවෘත කිරීම සහ ප්‍රකාශන ඉල්ලීම්.", 2, "ගිණුම් සේවා", 2 },
                    { 4, "புதிய கணக்குகள் மற்றும் அறிக்கை கோரிக்கைகள்.", 3, "கணக்கு சேவைகள்", 2 }
                });

            migrationBuilder.InsertData(
                table: "StaffUsers",
                columns: new[] { "Id", "CounterId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "sarah@smartq.bank", "Officer Sarah", true, "AQAAAAIAAYagAAAAE", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "sarah" });

            migrationBuilder.InsertData(
                table: "SubServices",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "DisplayOrder", "EstimatedServiceMinutes", "Icon", "IsActive", "Name", "ServiceId", "TokenPrefix", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "CASH_DEP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Deposit cash into personal or business accounts.", 1, 8, "savings", true, "Cash Deposit", 1, "CD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 2, "CASH_WD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "In-person cash withdrawals for amounts above ATM limits.", 2, 12, "atm", true, "Cash Withdrawal", 1, "CW", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 3, "CASH_TR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Transfer cash between accounts or to external beneficiaries.", 3, 5, "swap_horiz", true, "Cash Transfer", 1, "CT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 4, "FX", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Buy or sell foreign currency at competitive bank rates.", 4, 15, "currency_exchange", true, "Foreign Currency", 1, "FC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 5, "NA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Open a new savings or current account.", 1, 20, "person_add", true, "New Account Opening", 2, "NA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 6, "AU", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Update account profile and contact details.", 2, 10, "edit", true, "Account Update", 2, "AU", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 7, "SR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Request printed or digital account statements.", 3, 8, "receipt_long", true, "Statement Request", 2, "SR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 8, "PU", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Update passbook with recent transactions.", 4, 5, "menu_book", true, "Passbook Update", 2, "PU", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 9, "LN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "General loan information and eligibility check.", 1, 15, "help", true, "Loan Inquiry", 3, "LN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 10, "LA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Submit a new loan application.", 2, 30, "assignment", true, "Loan Application", 3, "LA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 11, "LP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Make loan installment payments.", 3, 10, "paid", true, "Loan Payment", 3, "LP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 12, "LD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Submit loan-related documents.", 4, 12, "upload_file", true, "Document Submission", 3, "LD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 13, "NC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Request a new debit or credit card.", 1, 15, "add_card", true, "New Card Request", 4, "NC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 14, "CR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Replace damaged or expired cards.", 2, 12, "credit_card", true, "Card Replacement", 4, "CR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 15, "PR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Reset card PIN at the branch.", 3, 8, "pin", true, "PIN Reset", 4, "PR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 16, "LC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Report lost or stolen cards.", 4, 10, "report", true, "Lost Card Complaint", 4, "LC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 17, "GEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "General banking inquiries.", 1, 10, "help_outline", true, "General Inquiry", 5, "GI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 18, "COMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Register a customer complaint.", 2, 15, "feedback", true, "Complaint", 5, "CP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 19, "DIG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), "Digital banking assistance.", 3, 12, "smartphone", true, "Digital Banking", 5, "DB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.InsertData(
                table: "VoiceTemplates",
                columns: new[] { "Id", "EventType", "IsActive", "LanguageId", "TemplateText" },
                values: new object[,]
                {
                    { 1, "TOKEN_CALLED", true, 1, "Token number {tokenNo}, please proceed to {counterName}" },
                    { 2, "TOKEN_CALLED", true, 2, "ටෝකන් අංකය {tokenNo}, කරුණාකර {counterName} වෙත එන්න" },
                    { 3, "TOKEN_CALLED", true, 3, "டோக்கன் எண் {tokenNo}, {counterName} க்குச் செல்லவும்" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounterServiceAssignments_CounterId_IsActive",
                table: "CounterServiceAssignments",
                columns: new[] { "CounterId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CounterServiceAssignments_ServiceId_IsActive",
                table: "CounterServiceAssignments",
                columns: new[] { "ServiceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyTokenSequences_SequenceDate_SubServiceId_TokenPrefix",
                table: "DailyTokenSequences",
                columns: new[] { "SequenceDate", "SubServiceId", "TokenPrefix" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyTokenSequences_SubServiceId",
                table: "DailyTokenSequences",
                column: "SubServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayMessages_LanguageId",
                table: "DisplayMessages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Code",
                table: "Services",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_IsActive_DisplayOrder",
                table: "Services",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTranslations_LanguageId",
                table: "ServiceTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTranslations_ServiceId",
                table: "ServiceTranslations",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffUsers_CounterId",
                table: "StaffUsers",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_SubServices_ServiceId_Code",
                table: "SubServices",
                columns: new[] { "ServiceId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubServices_ServiceId_IsActive_DisplayOrder",
                table: "SubServices",
                columns: new[] { "ServiceId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SubServices_TokenPrefix",
                table: "SubServices",
                column: "TokenPrefix");

            migrationBuilder.CreateIndex(
                name: "IX_SubServiceTranslations_LanguageId",
                table: "SubServiceTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SubServiceTranslations_SubServiceId",
                table: "SubServiceTranslations",
                column: "SubServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CounterId_Status",
                table: "Tokens",
                columns: new[] { "CounterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CreatedAt",
                table: "Tokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_LanguageId",
                table: "Tokens",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ServiceId_Status_CreatedAt",
                table: "Tokens",
                columns: new[] { "ServiceId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_Status_CreatedAt",
                table: "Tokens",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_SubServiceId_Status_CreatedAt",
                table: "Tokens",
                columns: new[] { "SubServiceId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenNo",
                table: "Tokens",
                column: "TokenNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenStatusHistories_TokenId_ChangedAt",
                table: "TokenStatusHistories",
                columns: new[] { "TokenId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceTemplates_LanguageId",
                table: "VoiceTemplates",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounterServiceAssignments");

            migrationBuilder.DropTable(
                name: "DailyTokenSequences");

            migrationBuilder.DropTable(
                name: "DisplayMessages");

            migrationBuilder.DropTable(
                name: "ServiceTranslations");

            migrationBuilder.DropTable(
                name: "StaffUsers");

            migrationBuilder.DropTable(
                name: "SubServiceTranslations");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TokenStatusHistories");

            migrationBuilder.DropTable(
                name: "VoiceTemplates");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "SubServices");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
