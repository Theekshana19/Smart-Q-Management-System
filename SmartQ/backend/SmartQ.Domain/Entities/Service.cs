using SmartQ.Domain.Common;

namespace SmartQ.Domain.Entities;

public class Service : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public ICollection<SubService> SubServices { get; set; } = new List<SubService>();
    public ICollection<ServiceTranslation> Translations { get; set; } = new List<ServiceTranslation>();
    public ICollection<CounterServiceAssignment> CounterAssignments { get; set; } = new List<CounterServiceAssignment>();
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
}
