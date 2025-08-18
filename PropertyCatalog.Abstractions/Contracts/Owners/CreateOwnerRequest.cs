namespace PropertyCatalog.Abstractions.Contracts.Owners;

public sealed class CreateOwnerRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Photo { get; set; }
    public DateTime? Birthday { get; set; }
}
