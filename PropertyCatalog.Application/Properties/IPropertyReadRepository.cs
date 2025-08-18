using PropertyCatalog.Abstractions.Contracts.Properties;
using PropertyCatalog.Abstractions.Primitives;

namespace PropertyCatalog.Application.Properties;

public interface IPropertyReadRepository
{
    Task<PagedResult<PropertyListItemDto>> SearchAsync(PropertySearchRequest request, CancellationToken ct);
    Task<PropertyDetailDto?> GetByIdAsync(string idProperty, CancellationToken ct);
}
