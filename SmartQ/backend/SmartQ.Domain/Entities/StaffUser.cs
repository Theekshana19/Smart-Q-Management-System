using SmartQ.Domain.Common;
using SmartQ.Domain.Enums;

namespace SmartQ.Domain.Entities;

public class StaffUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public int? CounterId { get; set; }
    public bool IsActive { get; set; }

    public Counter? Counter { get; set; }
}
