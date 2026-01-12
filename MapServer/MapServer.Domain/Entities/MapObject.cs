using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Domain.Entities;

/// <summary>
/// MongoDB document model for map objects (markers, vehicles, etc.).
/// Represents a document in the "objects" collection.
/// </summary>
public class MapObject
{
    /// <summary>
    /// The unique identifier (MongoDB's _id field).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// The location as a GeoJSON Point.
    /// Enables spatial queries and works with 2dsphere indexes.
    /// </summary>
    [BsonElement("location")]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = null!;

    /// <summary>
    /// The type of object (e.g., "Marker", "Jeep", "Tank").
    /// No server-side validation - client determines valid types.
    /// </summary>
    [BsonElement("objectType")]
    public string ObjectType { get; set; } = string.Empty;
}
