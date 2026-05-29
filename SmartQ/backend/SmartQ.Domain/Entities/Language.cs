namespace SmartQ.Domain.Entities;

public class Language
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ServiceTranslation> ServiceTranslations { get; set; } = new List<ServiceTranslation>();
    public ICollection<SubServiceTranslation> SubServiceTranslations { get; set; } = new List<SubServiceTranslation>();
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
    public ICollection<VoiceTemplate> VoiceTemplates { get; set; } = new List<VoiceTemplate>();
    public ICollection<DisplayMessage> DisplayMessages { get; set; } = new List<DisplayMessage>();
}
