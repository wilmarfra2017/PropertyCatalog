using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace PropertyCatalog.Infrastructure.Persistence.Mongo.Setup;

public sealed class MongoInitializerHostedService(IMongoDatabase db) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
        => await new MongoInitializer(db).EnsureAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
