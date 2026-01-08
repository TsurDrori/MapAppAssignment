// ============================================================================
// MapObject.cs - THE DATABASE MODEL FOR MAP OBJECTS
// ============================================================================
//
// WHAT: This is the Model for map objects (points with types like "Marker", "Jeep").
//       It represents a document in the "objects" collection in MongoDB.
//
// WHY:  Just like Polygon.cs, this has MongoDB-specific attributes and types
//       that the DTO layer doesn't need to know about.
//
// HOW IT'S STORED IN MONGODB:
//       {
//         "_id": ObjectId("507f1f77bcf86cd799439011"),
//         "location": {
//           "type": "Point",
//           "coordinates": [35.2, 32.5]
//         },
//         "objectType": "Marker"
//       }
//
// See BACKEND_CONCEPTS.md: Models, MongoDB Basics, Attributes (Square Brackets)
// ============================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Models;

// ============================================================================
// MapObject CLASS - MongoDB Document Model
// ============================================================================
public class MapObject
{
    // ========================================================================
    // Id - The unique identifier (same concept as Polygon.Id)
    // ========================================================================
    // [BsonId] = This is the document's _id field
    // [BsonRepresentation(BsonType.ObjectId)] = Store as ObjectId, use as string
    // "string?" = Nullable because MongoDB generates it when saving
    //
    // See Polygon.cs for detailed explanation of these attributes.
    // ========================================================================
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // ========================================================================
    // Location - WHERE this object is (a single point)
    // ========================================================================
    //
    // [BsonElement("location")]
    // Store this property as "location" in MongoDB (lowercase).
    //
    // GeoJsonPoint<GeoJson2DGeographicCoordinates>
    // A POINT in GeoJSON format. Simpler than polygon - just one coordinate.
    //
    // - GeoJsonPoint = A single point (not a shape)
    // - <GeoJson2DGeographicCoordinates> = Using lat/long on Earth
    //
    // IN MONGODB:
    // {
    //   "type": "Point",
    //   "coordinates": [35.2, 32.5]  // [longitude, latitude] - GeoJSON order!
    // }
    //
    // NOTE: GeoJSON uses [longitude, latitude] order (opposite of what you
    // might expect). This is handled by the Service layer during conversion.
    //
    // WHY GeoJsonPoint?
    // - Enables spatial queries (find all objects near a point)
    // - Works with MongoDB's 2dsphere index
    // - Standard format understood by mapping tools worldwide
    //
    // "= null!" = Will be set by Service before saving
    // ========================================================================
    [BsonElement("location")]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = null!;

    // ========================================================================
    // ObjectType - WHAT KIND of object this is
    // ========================================================================
    //
    // [BsonElement("objectType")]
    // Store as "objectType" in MongoDB (camelCase to match JSON conventions).
    //
    // This is just a simple string - no special MongoDB type needed.
    // Examples: "Marker", "Jeep", "Tank", "Building", etc.
    //
    // NO VALIDATION: The server stores whatever string the client sends.
    // The client application decides what types are valid.
    //
    // "= string.Empty" = Default to empty string (not null)
    // ========================================================================
    [BsonElement("objectType")]
    public string ObjectType { get; set; } = string.Empty;
}
