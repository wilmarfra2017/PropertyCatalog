namespace PropertyCatalog.Abstractions.Contracts.Properties;

public sealed class PropertyListItemDto
{
    public required string IdProperty { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public decimal Price { get; init; }
    public string? IdOwner { get; init; }
    public string? MainImageUrl { get; init; } 
}
