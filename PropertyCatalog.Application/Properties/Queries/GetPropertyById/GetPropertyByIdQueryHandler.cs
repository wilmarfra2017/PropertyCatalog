using MediatR;
using PropertyCatalog.Abstractions.Contracts.Properties;

namespace PropertyCatalog.Application.Properties.Queries.GetPropertyById;

public sealed class GetPropertyByIdQueryHandler
    : IRequestHandler<GetPropertyByIdQuery, PropertyDetailDto?>
{
    private readonly IPropertyReadRepository _repo;
    public GetPropertyByIdQueryHandler(IPropertyReadRepository repo) => _repo = repo;

    public Task<PropertyDetailDto?> Handle(GetPropertyByIdQuery request, CancellationToken ct) =>
        _repo.GetByIdAsync(request.IdProperty, ct);
}
