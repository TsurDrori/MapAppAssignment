// ============================================================================
// CreatePolygonRequest.cs - DATA FOR CREATING A NEW POLYGON
// ============================================================================
//
// WHAT: The data structure the client sends when creating a new polygon.
//       This is a "request DTO" - data coming IN to the server.
//
// WHY:  When the client calls POST /api/polygons with JSON, ASP.NET Core
//       automatically converts that JSON into this class (called "model binding").
//
// WHY NO ID FIELD?
//       The client doesn't provide an ID - MongoDB generates one automatically
//       when the document is saved. The ID is included in the RESPONSE.
//
// EXAMPLE REQUEST BODY:
//       POST /api/polygons
//       {
//         "coordinates": [
//           { "latitude": 32.5, "longitude": 35.2 },
//           { "latitude": 32.6, "longitude": 35.3 },
//           { "latitude": 32.5, "longitude": 35.3 }
//         ]
//       }
//
// VALIDATION:
//       Validation happens in PolygonService, not here. The service checks:
//       - At least 3 coordinates (minimum for a polygon)
//       - All coordinates are valid (lat: -90 to 90, lon: -180 to 180)
//       - Auto-closes the polygon if needed (adds first point at end)
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects)
// ============================================================================

namespace MapServer.DTOs;

// ============================================================================
// CreatePolygonRequest CLASS - Request DTO for creating polygons
// ============================================================================
public class CreatePolygonRequest
{
    // ========================================================================
    // Coordinates - The points that form the polygon
    // ========================================================================
    // The client provides a list of points. The server will:
    // 1. Validate there are at least 3 points
    // 2. Validate each coordinate is within valid ranges
    // 3. Automatically close the polygon if needed (add first point at end)
    // 4. Convert to MongoDB GeoJSON format
    // 5. Save to database
    //
    // "List<Coordinate>" = a list of Coordinate objects
    // "= new()" = start with an empty list (never null)
    //
    // NOTE: The polygon does NOT need to be pre-closed. If you send:
    //   [A, B, C] (3 points, not closed)
    // The server will save it as:
    //   [A, B, C, A] (4 points, closed)
    //
    // See BACKEND_CONCEPTS.md: Generic Types
    // ========================================================================
    public List<Coordinate> Coordinates { get; set; } = new();
}
