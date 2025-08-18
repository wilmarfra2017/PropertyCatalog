namespace PropertyCatalog.Infrastructure.Persistence.Mongo;

public sealed class MongoSettings
{
    public string ConnectionString { get; init; } = default!;
    public string Database { get; init; } = default!;
}
