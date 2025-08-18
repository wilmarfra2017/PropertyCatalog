using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PropertyCatalog.Domain.Entities;

namespace PropertyCatalog.Infrastructure.Persistence.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MongoSettings>(config.GetSection("Mongo"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(opt.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(opt.Database);
        });
        
        MongoConventions.Register();
        RegisterClassMaps();

        return services;
    }

    public static IServiceCollection AddMongoSimpleSetup(this IServiceCollection services)
    {
        services.AddHostedService<Setup.MongoInitializerHostedService>();
        return services;
    }

    private static void RegisterClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Owner)))
            BsonClassMap.RegisterClassMap<Owner>(cm => { cm.AutoMap(); cm.MapIdMember(x => x.IdOwner); cm.SetIgnoreExtraElements(true); });

        if (!BsonClassMap.IsClassMapRegistered(typeof(Property)))
            BsonClassMap.RegisterClassMap<Property>(cm => { cm.AutoMap(); cm.MapIdMember(x => x.IdProperty); cm.SetIgnoreExtraElements(true); });

        if (!BsonClassMap.IsClassMapRegistered(typeof(PropertyImage)))
            BsonClassMap.RegisterClassMap<PropertyImage>(cm => { cm.AutoMap(); cm.MapIdMember(x => x.IdPropertyImage); cm.SetIgnoreExtraElements(true); });

        if (!BsonClassMap.IsClassMapRegistered(typeof(PropertyTrace)))
            BsonClassMap.RegisterClassMap<PropertyTrace>(cm => { cm.AutoMap(); cm.MapIdMember(x => x.IdPropertyTrace); cm.SetIgnoreExtraElements(true); });
    }
}
