using MediatR;
using PropertyCatalog.Abstractions.Primitives;
using PropertyCatalog.Abstractions.Contracts.Properties;

namespace PropertyCatalog.Application.Properties.Queries.SearchProperties;

public sealed class SearchPropertiesQueryHandler
    : IRequestHandler<SearchPropertiesQuery, PagedResult<PropertyListItemDto>>
{
    private readonly IPropertyReadRepository _repo;
    public SearchPropertiesQueryHandler(IPropertyReadRepository repo) => _repo = repo;

    public Task<PagedResult<PropertyListItemDto>> Handle(
        SearchPropertiesQuery request,
        CancellationToken cancellationToken) =>
        _repo.SearchAsync(request.Request, cancellationToken);
}
