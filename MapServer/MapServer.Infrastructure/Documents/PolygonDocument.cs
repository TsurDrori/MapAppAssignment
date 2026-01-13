using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Infrastructure.Documents;

/// <summary>
/// MongoDB document model for polygons.
/// Handles BSON serialization - the persistence concern.
/// </summary>
public class PolygonDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("geometry")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; } = null!;
}
