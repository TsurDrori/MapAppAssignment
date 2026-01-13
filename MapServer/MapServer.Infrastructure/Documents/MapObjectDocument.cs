using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Infrastructure.Documents;

/// <summary>
/// MongoDB document model for map objects.
/// Handles BSON serialization - the persistence concern.
/// </summary>
public class MapObjectDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("location")]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = null!;

    [BsonElement("objectType")]
    public string ObjectType { get; set; } = string.Empty;
}
