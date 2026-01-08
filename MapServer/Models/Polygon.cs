// ============================================================================
// Polygon.cs - THE DATABASE MODEL FOR POLYGONS
// ============================================================================
//
// WHAT: This is the "Model" - the exact shape of data stored in MongoDB.
//       It represents a polygon document in the "polygons" collection.
//
// WHY:  This is different from PolygonDto because:
//       - DTO = what the API sends/receives (simple, no database stuff)
//       - Model = what's actually stored in MongoDB (has BSON attributes, GeoJSON types)
//
// HOW IT'S STORED IN MONGODB:
//       {
//         "_id": ObjectId("507f1f77bcf86cd799439011"),
//         "geometry": {
//           "type": "Polygon",
//           "coordinates": [[[35.2, 32.5], [35.3, 32.6], [35.3, 32.5], [35.2, 32.5]]]
//         }
//       }
//
// See BACKEND_CONCEPTS.md: Models, MongoDB Basics, Attributes (Square Brackets)
// ============================================================================

// These "using" statements import MongoDB-specific types
using MongoDB.Bson;                          // For BsonType enum (ObjectId, String, etc.)
using MongoDB.Bson.Serialization.Attributes; // For [BsonId], [BsonElement] attributes
using MongoDB.Driver.GeoJsonObjectModel;     // For GeoJsonPolygon type

// Models live in the MapServer.Models namespace
namespace MapServer.Models;

// ============================================================================
// Polygon CLASS - MongoDB Document Model
// ============================================================================
// This class maps directly to a MongoDB document.
// The MongoDB driver uses the [Bson...] attributes to know how to store/load data.
// ============================================================================
public class Polygon
{
    // ========================================================================
    // Id - The unique identifier (MongoDB's _id field)
    // ========================================================================
    //
    // [BsonId]
    // This attribute says "this property is the document's _id field".
    // Every MongoDB document MUST have an _id. It's the primary key.
    //
    // [BsonRepresentation(BsonType.ObjectId)]
    // This says "store it as an ObjectId in MongoDB, but use a string in C#".
    //
    // WHY STRING AND NOT ObjectId?
    // - Easier to work with (no conversion needed in most code)
    // - JSON serialization works naturally (ObjectId would need special handling)
    // - The representation attribute handles the conversion automatically
    //
    // "string?" - Nullable because MongoDB generates the ID when saving.
    // Before saving, the ID is null. After saving, MongoDB fills it in.
    //
    // EXAMPLE:
    //   C# Code:     Id = "507f1f77bcf86cd799439011" (string)
    //   MongoDB:     _id: ObjectId("507f1f77bcf86cd799439011") (ObjectId type)
    //
    // See BACKEND_CONCEPTS.md: Attributes (Square Brackets), Null Safety
    // ========================================================================
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // ========================================================================
    // Geometry - The polygon shape in GeoJSON format
    // ========================================================================
    //
    // [BsonElement("geometry")]
    // This attribute says "store this property as 'geometry' in MongoDB".
    // Without it, it would be stored as "Geometry" (capital G) by default.
    // Using lowercase matches MongoDB conventions and GeoJSON standard.
    //
    // GeoJsonPolygon<GeoJson2DGeographicCoordinates>
    // This is MongoDB's built-in type for storing polygon shapes. Let's break it down:
    //
    // - GeoJsonPolygon = A polygon shape (closed area)
    // - <GeoJson2DGeographicCoordinates> = Using lat/long coordinates on Earth
    //
    // WHY THIS COMPLEX TYPE?
    // - MongoDB can do spatial queries (find all polygons containing a point)
    // - The 2dsphere index we created works with this type
    // - It's the standard GeoJSON format used worldwide
    //
    // THE STRUCTURE:
    // GeoJsonPolygon contains:
    //   - Coordinates (GeoJsonPolygonCoordinates)
    //     - Exterior (the outer ring - GeoJsonLinearRingCoordinates)
    //       - Positions (list of GeoJson2DGeographicCoordinates)
    //         - Each position has Longitude and Latitude
    //
    // "= null!" - The "!" is a "null-forgiving operator". It tells the compiler:
    // "I know this looks like it could be null, but trust me, it will be set
    // before it's used." The Service layer always sets this before saving.
    //
    // WHY NOT "= new()"?
    // Creating a GeoJsonPolygon requires coordinates. We can't create an
    // "empty" one like we do with lists. The Service layer creates it
    // with actual coordinates from the request.
    //
    // See BACKEND_CONCEPTS.md: Generic Types, Null Safety, MongoDB Basics
    // ========================================================================
    [BsonElement("geometry")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; } = null!;
}
