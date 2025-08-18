using MediatR;
using PropertyCatalog.Abstractions.Contracts.Owners;

namespace PropertyCatalog.Application.Owners.Commands.CreateOwner;

public sealed record CreateOwnerCommand(
    string Name,
    string? Address,
    string? Photo,
    DateTime? Birthday
) : IRequest<OwnerSummaryDto>;
