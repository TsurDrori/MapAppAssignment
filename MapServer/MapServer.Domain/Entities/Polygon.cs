using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Domain.Entities;

/// <summary>
/// MongoDB document model for polygons.
/// Represents a polygon document in the "polygons" collection.
/// </summary>
public class Polygon
{
    /// <summary>
    /// The unique identifier (MongoDB's _id field).
    /// Stored as ObjectId in MongoDB, exposed as string in C#.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// The polygon shape in GeoJSON format.
    /// Enables spatial queries and works with 2dsphere indexes.
    /// </summary>
    [BsonElement("geometry")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; } = null!;
}
