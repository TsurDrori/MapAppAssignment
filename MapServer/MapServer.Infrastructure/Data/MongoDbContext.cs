using MapServer.Infrastructure.Configuration;
using MapServer.Infrastructure.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Data;

/// <summary>
/// Manages the MongoDB connection and provides access to collections.
/// Registered as a Singleton - one instance for the entire application lifetime.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        Polygons = _database.GetCollection<PolygonDocument>(settings.Value.PolygonsCollectionName);
        MapObjects = _database.GetCollection<MapObjectDocument>(settings.Value.ObjectsCollectionName);

        CreateIndexes();
    }

    /// <summary>
    /// The polygons collection.
    /// </summary>
    public IMongoCollection<PolygonDocument> Polygons { get; }

    /// <summary>
    /// The map objects collection.
    /// </summary>
    public IMongoCollection<MapObjectDocument> MapObjects { get; }

    /// <summary>
    /// Creates 2dsphere indexes for efficient geographic queries.
    /// </summary>
    private void CreateIndexes()
    {
        var polygonIndexKeys = Builders<PolygonDocument>.IndexKeys.Geo2DSphere(p => p.Geometry);
        Polygons.Indexes.CreateOne(new CreateIndexModel<PolygonDocument>(polygonIndexKeys));

        var objectIndexKeys = Builders<MapObjectDocument>.IndexKeys.Geo2DSphere(o => o.Location);
        MapObjects.Indexes.CreateOne(new CreateIndexModel<MapObjectDocument>(objectIndexKeys));
    }
}
