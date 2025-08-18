using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace PropertyCatalog.Infrastructure.Persistence.Mongo;

internal static class MongoConventions
{
    private static bool _applied;

    public static void Register()
    {
        if (_applied) return;

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String)
        };
        ConventionRegistry.Register("propertycatalog_conventions", pack, _ => true);
        
        BsonSerializer.RegisterSerializer(typeof(decimal), new MongoDB.Bson.Serialization.Serializers.DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(typeof(decimal?), new MongoDB.Bson.Serialization.Serializers.NullableSerializer<decimal>(
            new MongoDB.Bson.Serialization.Serializers.DecimalSerializer(BsonType.Decimal128)));

        _applied = true;
    }
}
