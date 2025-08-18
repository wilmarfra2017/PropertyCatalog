namespace PropertyCatalog.Domain.Entities;

public sealed class PropertyImage
{
    public string IdPropertyImage { get; init; } = default!;
    public string IdProperty { get; set; } = default!;
    public string File { get; set; } = default!;
    public bool Enabled { get; set; }
}
