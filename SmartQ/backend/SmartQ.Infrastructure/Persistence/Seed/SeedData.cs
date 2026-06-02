using Microsoft.EntityFrameworkCore;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;

namespace SmartQ.Infrastructure.Persistence.Seed;

public static class SeedData
{
    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedLanguages(modelBuilder);
        SeedServices(modelBuilder);
        SeedSubServices(modelBuilder);
        SeedTranslations(modelBuilder);
        SeedCounters(modelBuilder);
        SeedCounterAssignments(modelBuilder);
        SeedStaff(modelBuilder);
        SeedSettings(modelBuilder);
        SeedVoiceTemplates(modelBuilder);
        SeedDisplayMessages(modelBuilder);
    }

    private static void SeedLanguages(ModelBuilder mb)
    {
        mb.Entity<Language>().HasData(
            new Language { Id = 1, Code = "EN", Name = "English", NativeName = "English", IsDefault = true, IsActive = true, DisplayOrder = 1, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Language { Id = 2, Code = "SI", Name = "Sinhala", NativeName = "සිංහල", IsDefault = false, IsActive = true, DisplayOrder = 2, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Language { Id = 3, Code = "TA", Name = "Tamil", NativeName = "தமிழ்", IsDefault = false, IsActive = true, DisplayOrder = 3, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );
    }

    private static void SeedServices(ModelBuilder mb)
    {
        mb.Entity<Service>().HasData(
            new Service { Id = 1, Code = "CASH", Name = "Cash Services", Description = "Deposits, withdrawals, and currency exchange.", Icon = "payments", DisplayOrder = 1, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Service { Id = 2, Code = "ACC", Name = "Account Services", Description = "Open new accounts, update profiles, and statement requests.", Icon = "account_balance", DisplayOrder = 2, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Service { Id = 3, Code = "LOAN", Name = "Loan Services", Description = "Mortgages, personal loans, and credit consultations.", Icon = "description", DisplayOrder = 3, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Service { Id = 4, Code = "CARD", Name = "Card Services", Description = "New debit/credit cards, PIN resets, and card blockage.", Icon = "credit_card", DisplayOrder = 4, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Service { Id = 5, Code = "HELP", Name = "Customer Support", Description = "General inquiries, complaints, and digital banking assistance.", Icon = "support_agent", DisplayOrder = 5, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );
    }

    private static void SeedSubServices(ModelBuilder mb)
    {
        var subs = new List<SubService>
        {
            // Cash
            Sub(1, 1, "CASH_DEP", "Cash Deposit", "Deposit cash into personal or business accounts.", "CD", "savings", 8, 1),
            Sub(2, 1, "CASH_WD", "Cash Withdrawal", "In-person cash withdrawals for amounts above ATM limits.", "CW", "atm", 12, 2),
            Sub(3, 1, "CASH_TR", "Cash Transfer", "Transfer cash between accounts or to external beneficiaries.", "CT", "swap_horiz", 5, 3),
            Sub(4, 1, "FX", "Foreign Currency", "Buy or sell foreign currency at competitive bank rates.", "FC", "currency_exchange", 15, 4),
            // Account
            Sub(5, 2, "NA", "New Account Opening", "Open a new savings or current account.", "NA", "person_add", 20, 1),
            Sub(6, 2, "AU", "Account Update", "Update account profile and contact details.", "AU", "edit", 10, 2),
            Sub(7, 2, "SR", "Statement Request", "Request printed or digital account statements.", "SR", "receipt_long", 8, 3),
            Sub(8, 2, "PU", "Passbook Update", "Update passbook with recent transactions.", "PU", "menu_book", 5, 4),
            // Loan
            Sub(9, 3, "LN", "Loan Inquiry", "General loan information and eligibility check.", "LN", "help", 15, 1),
            Sub(10, 3, "LA", "Loan Application", "Submit a new loan application.", "LA", "assignment", 30, 2),
            Sub(11, 3, "LP", "Loan Payment", "Make loan installment payments.", "LP", "paid", 10, 3),
            Sub(12, 3, "LD", "Document Submission", "Submit loan-related documents.", "LD", "upload_file", 12, 4),
            // Card
            Sub(13, 4, "NC", "New Card Request", "Request a new debit or credit card.", "NC", "add_card", 15, 1),
            Sub(14, 4, "CR", "Card Replacement", "Replace damaged or expired cards.", "CR", "credit_card", 12, 2),
            Sub(15, 4, "PR", "PIN Reset", "Reset card PIN at the branch.", "PR", "pin", 8, 3),
            Sub(16, 4, "LC", "Lost Card Complaint", "Report lost or stolen cards.", "LC", "report", 10, 4),
            // Support
            Sub(17, 5, "GEN", "General Inquiry", "General banking inquiries.", "GI", "help_outline", 10, 1),
            Sub(18, 5, "COMP", "Complaint", "Register a customer complaint.", "CP", "feedback", 15, 2),
            Sub(19, 5, "DIG", "Digital Banking", "Digital banking assistance.", "DB", "smartphone", 12, 3)
        };
        mb.Entity<SubService>().HasData(subs);
    }

    private static SubService Sub(int id, int serviceId, string code, string name, string desc, string prefix, string icon, int mins, int order) =>
        new()
        {
            Id = id, ServiceId = serviceId, Code = code, Name = name, Description = desc,
            TokenPrefix = prefix, Icon = icon, EstimatedServiceMinutes = mins,
            DisplayOrder = order, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime
        };

    private static void SeedTranslations(ModelBuilder mb)
    {
        var serviceTranslations = new (int Id, int ServiceId, int LangId, string Name, string Desc)[]
        {
            (1, 1, 2, "මුදල් සේවා", "තැන්පතු, නිකුත් කිරීම් සහ මුදල් හුවමාරු."),
            (2, 1, 3, "பண சேவைகள்", "வைப்புகள், திரும்பப் பெறுதல்கள் மற்றும் நாணய பரிமாற்றம்."),
            (3, 2, 2, "ගිණුම් සේවා", "නව ගිණුම් විවෘත කිරීම, යාවත්කාලීන කිරීම් සහ ප්‍රකාශන ඉල්ලීම්."),
            (4, 2, 3, "கணக்கு சேவைகள்", "புதிய கணக்குகள், புதுப்பிப்புகள் மற்றும் அறிக்கை கோரிக்கைகள்."),
            (5, 3, 2, "ණය සේවා", "උපනිවේශ, පුද්ගලික ණය සහ ණය උපදේශන."),
            (6, 3, 3, "கடன் சேவைகள்", "அடமானம், தனிப்பட்ட கடன் மற்றும் கடன் ஆலோசனை."),
            (7, 4, 2, "කාඩ් සේවා", "නව කාඩ්, PIN යළි පිහිටුවීම් සහ කාඩ් අවහිර කිරීම."),
            (8, 4, 3, "அட்டை சேவைகள்", "புதிய அட்டை, PIN மீட்டமைப்பு மற்றும் அட்டை தடுப்பு."),
            (9, 5, 2, "පාරිභෝගික සහාය", "පොදු විමසීම්, පැමිණිලි සහ ඩිජිටල් බැංකු සහාය."),
            (10, 5, 3, "வாடிக்கையாளர் ஆதரவு", "பொது விசாரணைகள், புகார்கள் மற்றும் டிஜிட்டல் வங்கி உதவி.")
        };
        foreach (var t in serviceTranslations)
        {
            mb.Entity<ServiceTranslation>().HasData(new ServiceTranslation
            {
                Id = t.Id, ServiceId = t.ServiceId, LanguageId = t.LangId, Name = t.Name, Description = t.Desc
            });
        }

        var subTranslations = new (int Id, int SubId, int LangId, string Name, string Desc)[]
        {
            (1, 1, 2, "මුදල් තැන්පතු", "පුද්ගලික හෝ ව්‍යාපාරික ගිණුම්වලට මුදල් තැන්පත් කරන්න."),
            (2, 1, 3, "பண வைப்பு", "தனிப்பட்ட அல்லது வணிகக் கணக்குகளில் பணம் வைக்கவும்."),
            (3, 2, 2, "මුදල් නිකුත් කිරීම", "ATM සීමාවට වඩා වැඩි මුදල් නිකුත් කිරීම්."),
            (4, 2, 3, "பண எடுப்பு", "ATM வரம்பை விட அதிகமான பண எடுப்புகள்."),
            (5, 3, 2, "මුදල් මාරු කිරීම", "ගිණුම් අතර හෝ බාහිර ප්‍රතිලාභීන්ට මුදල් මාරු කිරීම."),
            (6, 3, 3, "பண பரிமாற்றம்", "கணக்குகளுக்கு இடையில் அல்லது வெளிப்புறர்களுக்கு பணம் அனுப்புதல்."),
            (7, 4, 2, "විදේශ මුදල්", "තරඟකාරී අනුපාතයන්හි විදේශ මුදල් මිලදී ගැනීම/විකිණීම."),
            (8, 4, 3, "வெளிநாட்டு நாணயம்", "போட்டி விகிதங்களில் வெளிநாட்டு நாணயம் வாங்க/விற்."),
            (9, 5, 2, "නව ගිණුම් විවෘත කිරීම", "නව ඉතිරිකිරීම් හෝ ධාවක ගිණුමක් විවෘත කරන්න."),
            (10, 5, 3, "புதிய கணக்கு திறப்பு", "புதிய சேமிப்பு அல்லது நடப்புக் கணக்கைத் திறக்கவும்."),
            (11, 6, 2, "ගිණුම් යාවත්කාලීන කිරීම", "ගිණුම් පැතිකඩ සහ සම්බන්ධතා තොරතුරු යාවත්කාලීන කරන්න."),
            (12, 6, 3, "கணக்கு புதுப்பிப்பு", "கணக்கு விவரங்களைப் புதுப்பிக்கவும்."),
            (13, 7, 2, "ප්‍රකාශන ඉල්ලීම", "මුද්‍රිත හෝ ඩිජිටල් ගිණුම් ප්‍රකාශන ඉල්ලන්න."),
            (14, 7, 3, "அறிக்கை கோரிக்கை", "அச்சு அல்லது டிஜிட்டல் அறிக்கையைக் கோருங்கள்."),
            (15, 8, 2, "පාස්පොත් යාවත්කාලීනය", "මෑත ගනුදෙනු සහිත පාස්පොත යාවත්කාලීන කරන්න."),
            (16, 8, 3, "பாஸ்புக் புதுப்பிப்பு", "சமீபத்திய பரிவர்த்தனைகளுடன் பாஸ்புக் புதுப்பிக்கவும்."),
            (17, 9, 2, "ණය විමසීම", "සාමාන්‍ය ණය තොරතුරු සහ සුදුසුකම් පරීක්ෂාව."),
            (18, 9, 3, "கடன் விசாரணை", "பொது கடன் தகவல் மற்றும் தகுதி சரிபார்ப்பு."),
            (19, 10, 2, "ණය අයදුම", "නව ණය අයදුමක් ඉදිරිපත් කරන්න."),
            (20, 10, 3, "கடன் விண்ணப்பம்", "புதிய கடன் விண்ணப்பத்தைச் சமர்ப்பிக்கவும்."),
            (21, 11, 2, "ණය ගෙවීම", "ණය වාරික ගෙවීම් කරන්න."),
            (22, 11, 3, "கடன் செலுத்துதல்", "கடன் தவணைகளைச் செலுத்துங்கள்."),
            (23, 12, 2, "ලේඛන ඉදිරිපත් කිරීම", "ණය සම්බන්ධ ලේඛන ඉදිරිපත් කරන්න."),
            (24, 12, 3, "ஆவண சமர்ப்பிப்பு", "கடன் தொடர்பான ஆவணங்களைச் சமர்ப்பிக்கவும்."),
            (25, 13, 2, "නව කාඩ් ඉල්ලීම", "නව ඩෙබිට්/ක්‍රෙඩිට් කාඩ් ඉල්ලන්න."),
            (26, 13, 3, "புதிய அட்டை கோரிக்கை", "புதிய டெபிட்/கிரெடிட் அட்டையைக் கோருங்கள்."),
            (27, 14, 2, "කාඩ් ප්‍රතිස්ථාපනය", "හානි වූ හෝ කල් ඉකුත් වූ කාඩ් ප්‍රතිස්ථාපනය."),
            (28, 14, 3, "அட்டை மாற்றம்", "சேதமடைந்த அல்லது காலாவதியான அட்டைகளை மாற்றவும்."),
            (29, 15, 2, "PIN යළි පිහිටුවීම", "ශාඛාවේදී කාඩ් PIN යළි පිහිටුවන්න."),
            (30, 15, 3, "PIN மீட்டமைப்பு", "கிளையில் அட்டை PIN மீட்டமைக்கவும்."),
            (31, 16, 2, "නැතිවූ කාඩ් පැමිණිලි", "නැතිවූ හෝ සොරකම් කළ කාඩ් වාර්තා කරන්න."),
            (32, 16, 3, "இழந்த அட்டை புகார்", "இழந்த அல்லது திருடப்பட்ட அட்டையைப் புகாரளிக்கவும்."),
            (33, 17, 2, "පොදු විමසීම", "පොදු බැංකු විමසීම්."),
            (34, 17, 3, "பொது விசாரணை", "பொது வங்கி விசாரணைகள்."),
            (35, 18, 2, "පැමිණිලි", "පාරිභෝගික පැමිණිල්ලක් ලියාපදිංචි කරන්න."),
            (36, 18, 3, "புகார்", "வாடிக்கையாளர் புகாரைப் பதிவு செய்யுங்கள்."),
            (37, 19, 2, "ඩිජිටල් බැංකුව", "ඩිජිටල් බැංකු සහාය."),
            (38, 19, 3, "டிஜிட்டல் வங்கி", "டிஜிட்டல் வங்கி உதவி.")
        };
        foreach (var t in subTranslations)
        {
            mb.Entity<SubServiceTranslation>().HasData(new SubServiceTranslation
            {
                Id = t.Id, SubServiceId = t.SubId, LanguageId = t.LangId, Name = t.Name, Description = t.Desc
            });
        }
    }

    private static void SeedCounters(ModelBuilder mb)
    {
        mb.Entity<Counter>().HasData(
            new Counter { Id = 1, CounterNo = "01", CounterName = "Counter 01", Status = CounterStatus.AVAILABLE, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Counter { Id = 2, CounterNo = "02", CounterName = "Counter 02", Status = CounterStatus.AVAILABLE, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Counter { Id = 3, CounterNo = "03", CounterName = "Counter 03", Status = CounterStatus.AVAILABLE, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Counter { Id = 4, CounterNo = "04", CounterName = "Counter 04", Status = CounterStatus.AVAILABLE, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime },
            new Counter { Id = 5, CounterNo = "05", CounterName = "Counter 05", Status = CounterStatus.AVAILABLE, IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime }
        );
    }

    private static void SeedCounterAssignments(ModelBuilder mb)
    {
        mb.Entity<CounterServiceAssignment>().HasData(
            new CounterServiceAssignment { Id = 1, CounterId = 1, ServiceId = 2, IsActive = true },
            new CounterServiceAssignment { Id = 2, CounterId = 2, ServiceId = 1, IsActive = true },
            new CounterServiceAssignment { Id = 3, CounterId = 3, ServiceId = 1, IsActive = true },
            new CounterServiceAssignment { Id = 4, CounterId = 4, ServiceId = 4, IsActive = true },
            new CounterServiceAssignment { Id = 5, CounterId = 5, ServiceId = 3, IsActive = true },
            new CounterServiceAssignment { Id = 6, CounterId = 1, ServiceId = 5, IsActive = true }
        );
    }

    private static void SeedStaff(ModelBuilder mb)
    {
        mb.Entity<StaffUser>().HasData(
            new StaffUser
            {
                Id = 1, FullName = "Officer Sarah", Username = "sarah", Email = "sarah@smartq.bank",
                PasswordHash = "AQAAAAIAAYagAAAAE", Role = StaffRole.STAFF, CounterId = 2,
                IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime
            },
            new StaffUser
            {
                Id = 2, FullName = "Admin User", Username = "admin", Email = "admin@smartq.bank",
                PasswordHash = "AQAAAAIAAYagAAAAE", Role = StaffRole.ADMIN, CounterId = null,
                IsActive = true, CreatedAt = SeedTime, UpdatedAt = SeedTime
            }
        );
    }

    private static void SeedSettings(ModelBuilder mb)
    {
        var settings = new[]
        {
            ("TOKEN_RESET_MODE", "DAILY", "string", "Daily token sequence reset mode"),
            ("ENABLE_VOICE_ANNOUNCEMENT", "true", "bool", "Enable voice announcements on display"),
            ("ENABLE_PRINT_TOKEN", "true", "bool", "Enable token printing on kiosk"),
            ("DISPLAY_RECENT_CALL_COUNT", "3", "int", "Number of recently called tokens on display"),
            ("DISPLAY_WAITING_QUEUE_COUNT", "5", "int", "Number of waiting tokens on display"),
            ("DEFAULT_ESTIMATED_WAIT_MINUTES", "8", "int", "Default estimated wait minutes"),
            ("KIOSK_AUTO_RETURN_SECONDS", "5", "int", "Kiosk auto return seconds after finish"),
            ("ENABLE_PRIORITY_QUEUE", "true", "bool", "Priority tokens called before standard"),
            ("BRANCH_ID", "BR-9904", "string", "Branch identifier"),
            ("BRANCH_NAME", "Branch Alpha", "string", "Branch display name"),
            ("KIOSK_VERSION", "v2.4", "string", "Kiosk software version"),
            ("WAIT_TIME_WARNING_MINUTES", "10", "int", "Waiting threshold warning in minutes"),
            ("STAFF_UI_THEME", "ENTERPRISE_TEAL", "string", "Staff UI theme"),
            ("TRANSFER_REGENERATE_TOKEN", "false", "bool", "Regenerate token number on transfer"),
            ("STAFF_AUTO_REFRESH_SECONDS", "10", "int", "Staff console auto refresh interval"),
            ("CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN", "true", "bool", "Lock call next while active token exists"),
            ("SYSTEM_ONLINE", "true", "bool", "System online/offline flag"),
            ("STAFF_TOKEN_ID_FORMAT", "TK-{id}", "string", "Active token id label format"),
            ("STAFF_MY_COUNTER_UPCOMING_COUNT", "3", "int", "Upcoming tokens shown on My Counter"),
            ("STAFF_QUEUE_PRESSURE_HIGH_THRESHOLD", "12", "int", "Waiting count threshold for high queue pressure"),
            ("STAFF_BREAK_ALLOWANCE_MINUTES", "60", "int", "Allowed break minutes per shift"),
            ("STAFF_BREAK_USED_MINUTES", "15", "int", "Break minutes used in current shift"),
            ("STAFF_SHIFT_END_TIME", "17:00", "string", "Shift end time (HH:mm)")
        };
        int id = 1;
        foreach (var (key, val, type, desc) in settings)
        {
            mb.Entity<SystemSetting>().HasData(new SystemSetting
            {
                Id = id++, SettingKey = key, SettingValue = val, DataType = type, Description = desc, IsActive = true
            });
        }
    }

    private static void SeedVoiceTemplates(ModelBuilder mb)
    {
        mb.Entity<VoiceTemplate>().HasData(
            new VoiceTemplate { Id = 1, LanguageId = 1, EventType = "TOKEN_CALLED", TemplateText = "Token number {tokenNo}, please proceed to {counterName}", IsActive = true },
            new VoiceTemplate { Id = 2, LanguageId = 2, EventType = "TOKEN_CALLED", TemplateText = "ටෝකන් අංකය {tokenNo}, කරුණාකර {counterName} වෙත එන්න", IsActive = true },
            new VoiceTemplate { Id = 3, LanguageId = 3, EventType = "TOKEN_CALLED", TemplateText = "டோக்கன் எண் {tokenNo}, {counterName} க்குச் செல்லவும்", IsActive = true }
        );
    }

    private static void SeedDisplayMessages(ModelBuilder mb)
    {
        mb.Entity<DisplayMessage>().HasData(
            new DisplayMessage { Id = 1, LanguageId = null, MessageKey = "TICKER_1", MessageText = "PLEASE PROCEED TO YOUR COUNTER WHEN YOUR TOKEN IS CALLED", IsActive = true, DisplayOrder = 1 },
            new DisplayMessage { Id = 2, LanguageId = null, MessageKey = "TICKER_2", MessageText = "DOWNLOAD THE SMARTQ APP FOR INSTANT NOTIFICATIONS", IsActive = true, DisplayOrder = 2 },
            new DisplayMessage { Id = 3, LanguageId = null, MessageKey = "TICKER_3", MessageText = "THANK YOU FOR PATIENTLY WAITING AT SMARTQ BANK", IsActive = true, DisplayOrder = 3 },
            new DisplayMessage { Id = 4, LanguageId = null, MessageKey = "MOBILE_BANKING", MessageText = "Scan QR on your receipt to track your turn live.", IsActive = true, DisplayOrder = 4 },
            new DisplayMessage { Id = 5, LanguageId = null, MessageKey = "STAFF_EMPTY_QUEUE_TITLE", MessageText = "No active token", IsActive = true, DisplayOrder = 5 },
            new DisplayMessage { Id = 6, LanguageId = null, MessageKey = "STAFF_EMPTY_QUEUE_DESCRIPTION", MessageText = "The counter is currently idle. Click Call Next to serve the next customer in the priority queue.", IsActive = true, DisplayOrder = 6 },
            new DisplayMessage { Id = 7, LanguageId = null, MessageKey = "STAFF_CALL_NEXT_LOCKED_MESSAGE", MessageText = "Complete or skip current token before calling next", IsActive = true, DisplayOrder = 7 },
            new DisplayMessage { Id = 8, LanguageId = null, MessageKey = "STAFF_TRANSFER_WARNING", MessageText = "Transferred token will leave this counter's queue and be added to the destination's waiting list immediately.", IsActive = true, DisplayOrder = 8 },
            new DisplayMessage { Id = 9, LanguageId = null, MessageKey = "STAFF_COUNTER_SERVES_MESSAGE", MessageText = "You are currently handling assigned services for this counter.", IsActive = true, DisplayOrder = 9 },
            new DisplayMessage { Id = 10, LanguageId = null, MessageKey = "STAFF_TV_BRANCH_MESSAGE", MessageText = "Public display shows branch-wide waiting and called tokens.", IsActive = true, DisplayOrder = 10 },
            new DisplayMessage { Id = 11, LanguageId = null, MessageKey = "STAFF_MY_COUNTER_GREETING", MessageText = "You are currently handling the priority queue for assigned services.", IsActive = true, DisplayOrder = 11 },
            new DisplayMessage { Id = 12, LanguageId = null, MessageKey = "STAFF_MY_COUNTER_IDLE", MessageText = "No active token. Go to Queue Console to call the next customer.", IsActive = true, DisplayOrder = 12 },
            new DisplayMessage { Id = 13, LanguageId = null, MessageKey = "STAFF_CUSTOMER_LABEL_VIP", MessageText = "Priority Member", IsActive = true, DisplayOrder = 13 },
            new DisplayMessage { Id = 14, LanguageId = null, MessageKey = "STAFF_CUSTOMER_LABEL_STANDARD", MessageText = "Regular Member", IsActive = true, DisplayOrder = 14 },
            new DisplayMessage { Id = 15, LanguageId = null, MessageKey = "STAFF_QUEUE_PRESSURE_HIGH", MessageText = "Queue pressure is high. Avoid taking breaks at this time.", IsActive = true, DisplayOrder = 15 },
            new DisplayMessage { Id = 16, LanguageId = null, MessageKey = "STAFF_QUEUE_PRESSURE_NORMAL", MessageText = "Queue pressure is moderate.", IsActive = true, DisplayOrder = 16 },
            new DisplayMessage { Id = 17, LanguageId = null, MessageKey = "STAFF_QUEUE_PRESSURE_LOW", MessageText = "Queue pressure is low.", IsActive = true, DisplayOrder = 17 },
            new DisplayMessage { Id = 18, LanguageId = null, MessageKey = "STAFF_COUNTER_STATUS_AVAILABLE_OK", MessageText = "Counter is now available.", IsActive = true, DisplayOrder = 18 },
            new DisplayMessage { Id = 19, LanguageId = null, MessageKey = "STAFF_COUNTER_STATUS_BUSY_OK", MessageText = "Counter marked as busy.", IsActive = true, DisplayOrder = 19 },
            new DisplayMessage { Id = 20, LanguageId = null, MessageKey = "STAFF_COUNTER_STATUS_BREAK_OK", MessageText = "Break started. Counter is offline.", IsActive = true, DisplayOrder = 20 },
            new DisplayMessage { Id = 21, LanguageId = null, MessageKey = "STAFF_COUNTER_STATUS_OFFLINE_OK", MessageText = "Counter marked offline.", IsActive = true, DisplayOrder = 21 },
            new DisplayMessage { Id = 22, LanguageId = null, MessageKey = "STAFF_COUNTER_STATUS_ACTIVE_BLOCK", MessageText = "Complete current token before setting available.", IsActive = true, DisplayOrder = 22 },
            new DisplayMessage { Id = 23, LanguageId = null, MessageKey = "STAFF_EFFICIENCY_TREND", MessageText = "+2% since last hour", IsActive = true, DisplayOrder = 23 },
            new DisplayMessage { Id = 24, LanguageId = null, MessageKey = "STAFF_SHIFT_ENDED", MessageText = "Shift ended", IsActive = true, DisplayOrder = 24 }
        );
    }
}
