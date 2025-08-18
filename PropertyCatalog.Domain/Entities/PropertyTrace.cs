namespace PropertyCatalog.Domain.Entities;

public sealed class PropertyTrace
{
    public string IdPropertyTrace { get; init; } = default!;
    public string IdProperty { get; set; } = default!;
    public DateTime DateSale { get; set; }
    public string Name { get; set; } = default!;
    public decimal Value { get; set; }
    public decimal Tax { get; set; }
}
