using MapServer.Domain.Entities;
using MapServer.Infrastructure.Configuration;
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

        Polygons = _database.GetCollection<Polygon>(settings.Value.PolygonsCollectionName);
        MapObjects = _database.GetCollection<MapObject>(settings.Value.ObjectsCollectionName);

        CreateIndexes();
    }

    /// <summary>
    /// The polygons collection.
    /// </summary>
    public IMongoCollection<Polygon> Polygons { get; }

    /// <summary>
    /// The map objects collection.
    /// </summary>
    public IMongoCollection<MapObject> MapObjects { get; }

    /// <summary>
    /// Creates 2dsphere indexes for efficient geographic queries.
    /// </summary>
    private void CreateIndexes()
    {
        var polygonIndexKeys = Builders<Polygon>.IndexKeys.Geo2DSphere(p => p.Geometry);
        Polygons.Indexes.CreateOne(new CreateIndexModel<Polygon>(polygonIndexKeys));

        var objectIndexKeys = Builders<MapObject>.IndexKeys.Geo2DSphere(o => o.Location);
        MapObjects.Indexes.CreateOne(new CreateIndexModel<MapObject>(objectIndexKeys));
    }
}
