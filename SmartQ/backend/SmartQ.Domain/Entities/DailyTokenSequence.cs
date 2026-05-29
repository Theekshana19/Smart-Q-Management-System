namespace SmartQ.Domain.Entities;

public class DailyTokenSequence
{
    public int Id { get; set; }
    public DateTime SequenceDate { get; set; }
    public int SubServiceId { get; set; }
    public string TokenPrefix { get; set; } = string.Empty;
    public int LastNumber { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SubService SubService { get; set; } = null!;
}
