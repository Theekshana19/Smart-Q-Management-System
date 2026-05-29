namespace SmartQ.Domain.Entities;

public class DisplayMessage
{
    public int Id { get; set; }
    public int? LanguageId { get; set; }
    public string MessageKey { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    public Language? Language { get; set; }
}
