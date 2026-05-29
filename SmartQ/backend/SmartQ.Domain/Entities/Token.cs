using SmartQ.Domain.Enums;

namespace SmartQ.Domain.Entities;

public class Token
{
    public int Id { get; set; }
    public string TokenNo { get; set; } = string.Empty;
    public string TokenPrefix { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    public int LanguageId { get; set; }
    public int ServiceId { get; set; }
    public int SubServiceId { get; set; }
    public int? CounterId { get; set; }
    public TokenStatus Status { get; set; }
    public TokenPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SkippedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int EstimatedWaitMinutes { get; set; }

    public Language Language { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public SubService SubService { get; set; } = null!;
    public Counter? Counter { get; set; }
    public ICollection<TokenStatusHistory> StatusHistory { get; set; } = new List<TokenStatusHistory>();
}
