// ============================================================================
// CreateMapObjectRequest.cs - DATA FOR CREATING A NEW MAP OBJECT
// ============================================================================
//
// WHAT: The data structure the client sends when creating a new map object.
//       This is a "request DTO" - data coming IN to the server.
//
// WHY:  When the client calls POST /api/objects with JSON, ASP.NET Core
//       automatically converts that JSON into this class.
//
// EXAMPLE REQUEST BODY:
//       POST /api/objects
//       {
//         "location": { "latitude": 32.5, "longitude": 35.2 },
//         "objectType": "Marker"
//       }
//
// VALIDATION:
//       Validation happens in MapObjectService:
//       - Location coordinates must be valid (lat: -90 to 90, lon: -180 to 180)
//       - ObjectType is NOT validated - any string is accepted
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects)
// ============================================================================

namespace MapServer.DTOs;

// ============================================================================
// CreateMapObjectRequest CLASS - Request DTO for creating map objects
// ============================================================================
public class CreateMapObjectRequest
{
    // ========================================================================
    // Location - WHERE to place this object on the map
    // ========================================================================
    // A single point with latitude and longitude.
    // This will be validated and converted to MongoDB GeoJSON format.
    //
    // "= new()" = default to a new empty Coordinate
    // ========================================================================
    public Coordinate Location { get; set; } = new();

    // ========================================================================
    // ObjectType - WHAT KIND of object this is
    // ========================================================================
    // A string label chosen by the client (e.g., "Marker", "Jeep", "Tank").
    //
    // NO VALIDATION: The server accepts any string. This is by design -
    // the client application decides what types are valid. The server
    // is just a data store.
    //
    // EXAMPLES:
    //   - "Marker" - a general map marker
    //   - "Jeep" - a vehicle
    //   - "Building" - a structure
    //   - Literally anything the client wants
    //
    // "= string.Empty" = default to empty string (not null)
    // ========================================================================
    public string ObjectType { get; set; } = string.Empty;
}
