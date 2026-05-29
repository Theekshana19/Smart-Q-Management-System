namespace SmartQ.Domain.Entities;

public class ServiceTranslation
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Service Service { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
