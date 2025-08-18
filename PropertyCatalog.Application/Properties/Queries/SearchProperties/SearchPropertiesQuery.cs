using MediatR;
using PropertyCatalog.Abstractions.Contracts.Properties;
using PropertyCatalog.Abstractions.Primitives;

namespace PropertyCatalog.Application.Properties.Queries.SearchProperties;

public sealed record SearchPropertiesQuery(PropertySearchRequest Request)
    : IRequest<PagedResult<PropertyListItemDto>>;
