namespace SmartQ.Domain.Entities;

public class VoiceTemplate
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string TemplateText { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public Language Language { get; set; } = null!;
}
