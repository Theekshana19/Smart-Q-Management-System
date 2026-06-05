using Microsoft.EntityFrameworkCore;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Seed;

public static class ConfigurationDataSeeder
{
    private static readonly (string Key, string Value, string DataType, string Description)[] Settings =
    [
        ("BRANCH_NAME", "SmartQ Sri Lanka", "STRING", "Branch display name"),
        ("BANK_NAME", "SmartQ Bank", "STRING", "Bank display name"),
        ("ENABLE_PRIORITY_QUEUE", "true", "BOOLEAN", "Priority tokens called before standard"),
        ("WAIT_TIME_WARNING_MINUTES", "10", "NUMBER", "Waiting threshold warning in minutes"),
        ("ENABLE_VOICE_ANNOUNCEMENT", "true", "BOOLEAN", "Enable voice announcements on display"),
        ("ENABLE_PRINT_TOKEN", "true", "BOOLEAN", "Enable token printing on kiosk"),
        ("DISPLAY_RECENT_CALL_COUNT", "3", "NUMBER", "Recently called tokens on display"),
        ("DISPLAY_WAITING_QUEUE_COUNT", "5", "NUMBER", "Waiting tokens on display"),
        ("KIOSK_AUTO_RETURN_SECONDS", "5", "NUMBER", "Kiosk auto return seconds after finish"),
        ("STAFF_AUTO_REFRESH_SECONDS", "10", "NUMBER", "Staff console auto refresh interval"),
        ("CALL_NEXT_LOCK_WHEN_ACTIVE_TOKEN", "true", "BOOLEAN", "Lock call next while active token exists"),
        ("SATISFACTION_RATE", "4.9", "DECIMAL", "Customer satisfaction rate"),
        ("COUNTER_FEEDBACK_SCORE", "4.8", "DECIMAL", "Counter feedback score"),
        ("TOKEN_RESET_MODE", "DAILY", "STRING", "Daily token sequence reset mode"),
        ("TRANSFER_REGENERATE_TOKEN", "false", "BOOLEAN", "Regenerate token number on transfer")
    ];

    private static readonly (string Key, string Text, int Order)[] DisplayMessages =
    [
        ("KIOSK_LANGUAGE_HELP", "Touch your preferred language to continue", 100),
        ("KIOSK_SELECT_SERVICE_TITLE", "Select Your Service", 101),
        ("KIOSK_SELECT_SERVICE_SUBTITLE", "Please choose a category to receive your queue ticket.", 102),
        ("TOKEN_SUCCESS_TITLE", "Your Token is Ready", 103),
        ("TOKEN_SUCCESS_INSTRUCTION", "Please wait until your token is displayed and announced.", 104),
        ("DISPLAY_NOW_SERVING", "Now Serving", 105),
        ("DISPLAY_WAITING_QUEUE", "Waiting Queue", 106),
        ("DISPLAY_SCROLL_MESSAGE", "Please proceed to your counter when your token is called.", 107),
        ("DISPLAY_PROMO_TITLE", "SMART MOBILE BANKING", 107),
        ("STAFF_EMPTY_QUEUE_TITLE", "No tokens waiting", 108),
        ("STAFF_EMPTY_QUEUE_DESCRIPTION", "There are currently no customers in the queue for this counter's services.", 109),
        ("STAFF_CALL_NEXT_LOCKED_MESSAGE", "Complete or skip current token before calling next.", 110),
        ("STAFF_TRANSFER_WARNING", "Transferred token will leave this counter's queue and be added to the destination's waiting list immediately.", 111),
        ("STAFF_COUNTER_SERVES_MESSAGE", "Call Next only picks tokens assigned to this counter's services.", 112),
        ("ADMIN_ASSIGNMENT_WARNING", "Changing assignments affects which tokens staff can call.", 113),
        ("STAFF_PERF_CHART_EMPTY", "No hourly traffic data for this period.", 114),
        ("STAFF_PERF_CHART_NO_ACTIVITY", "No served tokens yet today. Chart updates when tokens are called or completed.", 115),
        ("STAFF_PERF_TIMELINE_EMPTY", "No counter activity for this period. Call or complete tokens in Queue Console to populate the timeline.", 116),
        ("STAFF_TOKEN_HISTORY_EMPTY", "No token history found for the selected filters.", 117),
        ("STAFF_NOTIFICATIONS_EMPTY", "No notifications.", 118)
    ];

    public static async Task SeedAsync(SmartQDbContext db, CancellationToken ct = default)
    {
        foreach (var (key, value, dataType, desc) in Settings)
        {
            if (!await db.SystemSettings.AnyAsync(s => s.SettingKey == key, ct))
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    DataType = dataType,
                    Description = desc,
                    IsActive = true
                });
            }
        }

        foreach (var (key, text, order) in DisplayMessages)
        {
            if (!await db.DisplayMessages.AnyAsync(m => m.MessageKey == key && m.LanguageId == null, ct))
            {
                db.DisplayMessages.Add(new DisplayMessage
                {
                    LanguageId = null,
                    MessageKey = key,
                    MessageText = text,
                    IsActive = true,
                    DisplayOrder = order
                });
            }
        }

        var enId = await db.Languages.Where(l => l.Code == "EN").Select(l => l.Id).FirstOrDefaultAsync(ct);
        var siId = await db.Languages.Where(l => l.Code == "SI").Select(l => l.Id).FirstOrDefaultAsync(ct);
        var taId = await db.Languages.Where(l => l.Code == "TA").Select(l => l.Id).FirstOrDefaultAsync(ct);

        if (enId > 0 && !await db.VoiceTemplates.AnyAsync(v => v.LanguageId == enId && v.EventType == "TOKEN_RECALLED", ct))
        {
            db.VoiceTemplates.Add(new VoiceTemplate
            {
                LanguageId = enId,
                EventType = "TOKEN_RECALLED",
                TemplateText = "Token number {tokenNo}, please proceed to {counterName}",
                IsActive = true
            });
        }

        if (siId > 0)
        {
            var siText = "ටෝකන් අංක {tokenNo}, කරුණාකර {counterName} වෙත යන්න";
            var existing = await db.VoiceTemplates.FirstOrDefaultAsync(v => v.LanguageId == siId && v.EventType == "TOKEN_CALLED", ct);
            if (existing != null && existing.TemplateText.Contains("එන්න"))
                existing.TemplateText = siText;
            else if (existing == null)
                db.VoiceTemplates.Add(new VoiceTemplate { LanguageId = siId, EventType = "TOKEN_CALLED", TemplateText = siText, IsActive = true });
        }

        if (taId > 0)
        {
            var taText = "டோக்கன் எண் {tokenNo}, தயவுசெய்து {counterName} செல்லவும்";
            var existing = await db.VoiceTemplates.FirstOrDefaultAsync(v => v.LanguageId == taId && v.EventType == "TOKEN_CALLED", ct);
            if (existing != null && existing.TemplateText.Contains("க்குச்"))
                existing.TemplateText = taText;
            else if (existing == null)
                db.VoiceTemplates.Add(new VoiceTemplate { LanguageId = taId, EventType = "TOKEN_CALLED", TemplateText = taText, IsActive = true });
        }

        await db.SaveChangesAsync(ct);
    }
}
