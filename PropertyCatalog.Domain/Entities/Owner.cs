namespace PropertyCatalog.Domain.Entities;

public sealed class Owner
{
    public string IdOwner { get; init; } = default!;
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? Photo { get; set; }
    public DateTime? Birthday { get; set; }
}
