using SmartQ.Domain.Common;

namespace SmartQ.Domain.Entities;

public class SubService : BaseEntity
{
    public int ServiceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TokenPrefix { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int EstimatedServiceMinutes { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public Service Service { get; set; } = null!;
    public ICollection<SubServiceTranslation> Translations { get; set; } = new List<SubServiceTranslation>();
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
    public ICollection<DailyTokenSequence> DailySequences { get; set; } = new List<DailyTokenSequence>();
}
