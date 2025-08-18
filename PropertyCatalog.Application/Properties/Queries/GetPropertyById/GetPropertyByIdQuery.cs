using MediatR;
using PropertyCatalog.Abstractions.Contracts.Properties;

namespace PropertyCatalog.Application.Properties.Queries.GetPropertyById;

public sealed record GetPropertyByIdQuery(string IdProperty)
    : IRequest<PropertyDetailDto?>;
