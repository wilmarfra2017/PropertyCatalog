using MediatR;
using PropertyCatalog.Abstractions.Contracts.Owners;
using PropertyCatalog.Application.Owners.Commands.CreateOwner;
using PropertyCatalog.Domain.Entities;

namespace PropertyCatalog.Application.Owners.Commands;

public sealed class CreateOwnerCommandHandler
    : IRequestHandler<CreateOwnerCommand, OwnerSummaryDto>
{
    private readonly IOwnerWriteRepository _repo;

    public CreateOwnerCommandHandler(IOwnerWriteRepository repo) => _repo = repo;

    public async Task<OwnerSummaryDto> Handle(CreateOwnerCommand request, CancellationToken ct)
    {        
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nombre es requerido.", nameof(request.Name));
        if (name.Length > 200)
            throw new ArgumentException("El nombre es demasiado largo (máx. 200).", nameof(request.Name));
        
        var addr = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        var photo = string.IsNullOrWhiteSpace(request.Photo) ? null : request.Photo.Trim();

        var owner = new Owner
        {            
            Name = name,
            Address = addr,
            Photo = photo,
            Birthday = request.Birthday
        };

        var id = await _repo.CreateAsync(owner, ct);

        return new OwnerSummaryDto
        {
            IdOwner = id,
            Name = name
        };
    }
}
