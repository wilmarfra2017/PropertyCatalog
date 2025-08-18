namespace PropertyCatalog.Application.Owners;

using PropertyCatalog.Domain.Entities;

public interface IOwnerWriteRepository
{
    Task<string> CreateAsync(Owner owner, CancellationToken ct);

    Task<bool> ExistsAsync(string name, DateTime? birthday, CancellationToken ct);
}
