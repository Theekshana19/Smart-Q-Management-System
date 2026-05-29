using SmartQ.Domain.Enums;

namespace SmartQ.Domain.Entities;

public class TokenStatusHistory
{
    public int Id { get; set; }
    public int TokenId { get; set; }
    public TokenStatus? OldStatus { get; set; }
    public TokenStatus NewStatus { get; set; }
    public int? CounterId { get; set; }
    public int? StaffUserId { get; set; }
    public string? Remarks { get; set; }
    public DateTime ChangedAt { get; set; }

    public Token Token { get; set; } = null!;
}
