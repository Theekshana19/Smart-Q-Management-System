namespace SmartQ.Domain.Entities;

public class CounterServiceAssignment
{
    public int Id { get; set; }
    public int CounterId { get; set; }
    public int ServiceId { get; set; }
    public bool IsActive { get; set; }

    public Counter Counter { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
