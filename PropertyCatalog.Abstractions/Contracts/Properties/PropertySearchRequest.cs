namespace PropertyCatalog.Abstractions.Contracts.Properties;

public sealed class PropertySearchRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public string? OwnerId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

