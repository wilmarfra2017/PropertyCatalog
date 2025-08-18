namespace PropertyCatalog.Domain.Entities;

public sealed class Property
{
    public string IdProperty { get; init; } = default!;
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public decimal Price { get; set; }
    public string CodeInternal { get; set; } = default!;
    public int Year { get; set; }
    public string IdOwner { get; set; } = default!;
}

