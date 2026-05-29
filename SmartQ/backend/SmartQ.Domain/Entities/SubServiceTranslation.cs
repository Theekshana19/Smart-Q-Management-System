namespace SmartQ.Domain.Entities;

public class SubServiceTranslation
{
    public int Id { get; set; }
    public int SubServiceId { get; set; }
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public SubService SubService { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
