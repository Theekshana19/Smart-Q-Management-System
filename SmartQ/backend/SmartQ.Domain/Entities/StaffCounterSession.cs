using SmartQ.Domain.Enums;

namespace SmartQ.Domain.Entities;

public class StaffCounterSession
{
    public int Id { get; set; }
    public int StaffUserId { get; set; }
    public int CounterId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public StaffCounterSessionStatus Status { get; set; }
    public string? LoginIp { get; set; }
    public string? DeviceName { get; set; }
    public string? Remarks { get; set; }

    public StaffUser StaffUser { get; set; } = null!;
    public Counter Counter { get; set; } = null!;
}
