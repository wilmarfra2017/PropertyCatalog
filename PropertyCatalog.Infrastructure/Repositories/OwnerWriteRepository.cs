using MongoDB.Driver;
using PropertyCatalog.Application.Owners;
using PropertyCatalog.Domain.Entities;

namespace PropertyCatalog.Infrastructure.Repositories;

public sealed class OwnerWriteRepository : IOwnerWriteRepository
{
    private readonly IMongoCollection<Owner> _owners;

    public OwnerWriteRepository(IMongoDatabase db)
        => _owners = db.GetCollection<Owner>("owners");

    public async Task<bool> ExistsAsync(string name, DateTime? birthday, CancellationToken ct)
    {
        var nameNorm = (name ?? string.Empty).Trim();
        var fb = Builders<Owner>.Filter;

        var filter = fb.Eq(o => o.Name, nameNorm);
        if (birthday.HasValue)
            filter = fb.And(filter, fb.Eq(o => o.Birthday, birthday.Value));

        return await _owners.Find(filter).Limit(1).AnyAsync(ct);
    }

    public async Task<string> CreateAsync(Owner owner, CancellationToken ct)
    {
        if (owner is null) throw new ArgumentNullException(nameof(owner));
        
        var name = owner.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Propietario.Nombre es requerido", nameof(owner));

        var address = string.IsNullOrWhiteSpace(owner.Address) ? null : owner.Address!.Trim();
        var photo = string.IsNullOrWhiteSpace(owner.Photo) ? null : owner.Photo!.Trim();
        
        if (await ExistsAsync(name, owner.Birthday, ct))
            throw new InvalidOperationException("Ya existe un propietario con el mismo nombre y fecha de nacimiento.");
        
        var id = string.IsNullOrWhiteSpace(owner.IdOwner)
            ? $"own-{Guid.NewGuid():N}"
            : owner.IdOwner!;

        var toInsert = new Owner
        {
            IdOwner = id,
            Name = name,
            Address = address,
            Photo = photo,
            Birthday = owner.Birthday
        };

        await _owners.InsertOneAsync(toInsert, cancellationToken: ct);
        return toInsert.IdOwner!;
    }
}
