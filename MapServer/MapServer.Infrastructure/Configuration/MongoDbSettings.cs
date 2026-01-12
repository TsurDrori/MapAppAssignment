namespace MapServer.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for MongoDB connection.
/// Maps to the "MongoDbSettings" section in appsettings.json.
/// </summary>
public class MongoDbSettings
{
    /// <summary>
    /// The MongoDB connection string (e.g., "mongodb://localhost:27017").
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name of the database to use (e.g., "MapServerDb").
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the polygons collection (e.g., "polygons").
    /// </summary>
    public string PolygonsCollectionName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the map objects collection (e.g., "objects").
    /// </summary>
    public string ObjectsCollectionName { get; set; } = string.Empty;
}
