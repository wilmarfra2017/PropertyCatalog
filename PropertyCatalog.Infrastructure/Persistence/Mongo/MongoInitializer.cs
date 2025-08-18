using MongoDB.Bson;
using MongoDB.Driver;

namespace PropertyCatalog.Infrastructure.Persistence.Mongo.Setup;

public sealed class MongoInitializer(IMongoDatabase db)
{
    private static readonly string[] Collections =
    {
        "owners", "properties", "propertyImages", "propertyTraces"
    };

    public async Task EnsureAsync(CancellationToken ct)
    {
        await CreateCollectionsIfMissing(ct);
        await EnsureIndexes(ct);        
    }

    private async Task CreateCollectionsIfMissing(CancellationToken ct)
    {
        var existing = await (await db.ListCollectionNamesAsync(cancellationToken: ct)).ToListAsync(ct);
        foreach (var name in Collections)
        {
            if (!existing.Contains(name))
                await db.CreateCollectionAsync(name, cancellationToken: ct);
        }
    }

    private async Task EnsureIndexes(CancellationToken ct)
    {
        await EnsurePropertyIndexes(ct);
        await EnsurePropertyImagesIndexes(ct);
        await EnsurePropertyTracesIndexes(ct);
        await EnsureOwnersIndexes(ct);
    }

    private async Task EnsurePropertyIndexes(CancellationToken ct)
    {
        var c = db.GetCollection<BsonDocument>("properties");

        var models = new List<CreateIndexModel<BsonDocument>>
        {            
            new(Builders<BsonDocument>.IndexKeys.Ascending("codeInternal"),
                new CreateIndexOptions { Unique = true }),
            
            new(Builders<BsonDocument>.IndexKeys.Ascending("idOwner")),
            
            new(Builders<BsonDocument>.IndexKeys.Ascending("price")),
            new(Builders<BsonDocument>.IndexKeys.Ascending("year")),
            new(Builders<BsonDocument>.IndexKeys.Ascending("price").Ascending("year")),
            
            new(Builders<BsonDocument>.IndexKeys.Text("name").Text("address"))
        };

        await c.Indexes.CreateManyAsync(models, ct);
    }

    private async Task EnsurePropertyImagesIndexes(CancellationToken ct)
    {
        var c = db.GetCollection<BsonDocument>("propertyImages");

        var baseModels = new List<CreateIndexModel<BsonDocument>>
    {
        new(Builders<BsonDocument>.IndexKeys.Ascending("idProperty")),
        new(Builders<BsonDocument>.IndexKeys.Ascending("enabled"))
    };
        await c.Indexes.CreateManyAsync(baseModels, cancellationToken: ct);

        var keys = Builders<BsonDocument>.IndexKeys
            .Ascending("idProperty")
            .Ascending("enabled");

        var options = new CreateIndexOptions<BsonDocument>
        {
            Unique = true,
            PartialFilterExpression = Builders<BsonDocument>.Filter.Eq("enabled", true)
        };

        await c.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(keys, options),
            cancellationToken: ct);
    }

    private async Task EnsurePropertyTracesIndexes(CancellationToken ct)
    {
        var c = db.GetCollection<BsonDocument>("propertyTraces");

        await c.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("idProperty").Descending("dateSale")
            ),
            cancellationToken: ct);
    }

    private async Task EnsureOwnersIndexes(CancellationToken ct)
    {
        var c = db.GetCollection<BsonDocument>("owners");
        await c.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("name")),
            cancellationToken: ct);
    }
}
