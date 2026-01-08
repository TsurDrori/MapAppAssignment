// ============================================================================
// PolygonDto.cs - POLYGON DATA FOR API RESPONSES
// ============================================================================
//
// WHAT: The data structure sent TO the client when they request polygons.
//       "Dto" = Data Transfer Object = designed for sending data over the wire.
//
// WHY:  When the client calls GET /api/polygons, this is what they receive.
//       It's a "response DTO" - data going OUT from the server.
//
// WHY SEPARATE FROM THE MODEL?
//       The Model (Polygon.cs) has MongoDB-specific stuff (BsonId, GeoJsonPolygon).
//       The client doesn't need to know about that. They just want:
//       - An ID (to reference it later for deletion)
//       - A list of coordinates (to draw on the map)
//
// EXAMPLE JSON RESPONSE:
//       {
//         "id": "507f1f77bcf86cd799439011",
//         "coordinates": [
//           { "latitude": 32.5, "longitude": 35.2 },
//           { "latitude": 32.6, "longitude": 35.3 },
//           { "latitude": 32.5, "longitude": 35.3 },
//           { "latitude": 32.5, "longitude": 35.2 }
//         ]
//       }
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects)
// ============================================================================

namespace MapServer.DTOs;

// ============================================================================
// PolygonDto CLASS - Response DTO for polygons
// ============================================================================
public class PolygonDto
{
    // ========================================================================
    // Id - The unique identifier for this polygon
    // ========================================================================
    // This is the MongoDB ObjectId converted to a string.
    // The client uses this to reference the polygon (e.g., for deletion).
    //
    // "string?" = string that CAN be null (the ? means nullable)
    //
    // WHY NULLABLE?
    // When creating a NEW polygon, the ID doesn't exist yet - MongoDB
    // generates it. So when mapping to a DTO before saving, Id would be null.
    // After saving, MongoDB assigns an ID and we include it in the response.
    //
    // See BACKEND_CONCEPTS.md: Null Safety
    // ========================================================================
    public string? Id { get; set; }

    // ========================================================================
    // Coordinates - The list of points that form this polygon
    // ========================================================================
    // A polygon is just a closed shape made of connected points.
    // The first and last coordinate should be the same (closing the shape).
    //
    // "List<Coordinate>" = a list (array) of Coordinate objects
    //   - List is a C# collection type (like Array in JavaScript)
    //   - <Coordinate> means "each item is a Coordinate"
    //
    // "= new()" = initialize with an empty list (not null)
    //   - This is C# 9+ shorthand for "= new List<Coordinate>()"
    //   - Avoids null reference exceptions
    //
    // See BACKEND_CONCEPTS.md: Generic Types, Null Safety
    // ========================================================================
    public List<Coordinate> Coordinates { get; set; } = new();
}
