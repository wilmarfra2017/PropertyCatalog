using PropertyCatalog.Abstractions.Contracts.Owners;

namespace PropertyCatalog.Abstractions.Contracts.Properties;

public sealed class PropertyDetailDto
{
    public required string IdProperty { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public decimal Price { get; init; }
    public required string CodeInternal { get; init; }
    public int Year { get; init; }
    public OwnerSummaryDto? Owner { get; init; }
    public string? MainImageUrl { get; init; }
    public IReadOnlyList<string>? OtherImageUrls { get; init; }    
    public int? SalesCount { get; init; }
    public DateTime? LastSaleDate { get; init; }
    public decimal? LastSaleValue { get; init; }
}
