using SmartQ.Domain.Common;
using SmartQ.Domain.Enums;

namespace SmartQ.Domain.Entities;

public class Counter : BaseEntity
{
    public string CounterNo { get; set; } = string.Empty;
    public string CounterName { get; set; } = string.Empty;
    public CounterStatus Status { get; set; }
    public bool IsActive { get; set; }

    public ICollection<CounterServiceAssignment> ServiceAssignments { get; set; } = new List<CounterServiceAssignment>();
    public ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
}
