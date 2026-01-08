// ============================================================================
// MapObjectDto.cs - MAP OBJECT DATA FOR API RESPONSES
// ============================================================================
//
// WHAT: The data structure sent TO the client when they request map objects.
//       A "map object" is a point on the map with a type (like "Marker", "Jeep").
//
// WHY:  When the client calls GET /api/objects, this is what they receive.
//       It contains everything needed to display the object on the map.
//
// DIFFERENCE FROM POLYGON:
//       - Polygon = a shape (list of coordinates)
//       - MapObject = a single point with a type label
//
// EXAMPLE JSON RESPONSE:
//       {
//         "id": "507f1f77bcf86cd799439011",
//         "location": { "latitude": 32.5, "longitude": 35.2 },
//         "objectType": "Marker"
//       }
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects)
// ============================================================================

namespace MapServer.DTOs;

// ============================================================================
// MapObjectDto CLASS - Response DTO for map objects
// ============================================================================
public class MapObjectDto
{
    // ========================================================================
    // Id - The unique identifier for this object
    // ========================================================================
    // Same concept as PolygonDto.Id - the MongoDB ObjectId as a string.
    // Used by the client to reference this object (e.g., for deletion).
    //
    // "string?" = nullable string (can be null before saving to database)
    //
    // See BACKEND_CONCEPTS.md: Null Safety
    // ========================================================================
    public string? Id { get; set; }

    // ========================================================================
    // Location - WHERE this object is on the map
    // ========================================================================
    // A single geographic point (latitude/longitude).
    //
    // "Coordinate" = our simple DTO class (not MongoDB's GeoJsonPoint)
    // "= new()" = initialize with a new Coordinate (avoids null)
    //
    // The default Coordinate has Latitude=0 and Longitude=0 (in the ocean
    // off the coast of Africa), but this is just a safety default - real
    // data will always come from the request.
    // ========================================================================
    public Coordinate Location { get; set; } = new();

    // ========================================================================
    // ObjectType - WHAT KIND of object this is
    // ========================================================================
    // A string label like "Marker", "Jeep", "Tank", etc.
    //
    // WHY A STRING AND NOT AN ENUM?
    // The assignment says the client determines valid types. Using a string
    // means the client can create any type they want without server changes.
    // The server doesn't care what the type is - it just stores it.
    //
    // "= string.Empty" = default to empty string (not null)
    //   - string.Empty is the same as "" but more explicit
    //   - Avoids null reference issues
    // ========================================================================
    public string ObjectType { get; set; } = string.Empty;
}
