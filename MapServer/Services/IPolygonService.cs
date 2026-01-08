// ============================================================================
// IPolygonService.cs - THE CONTRACT FOR POLYGON BUSINESS LOGIC
// ============================================================================
//
// WHAT: Interface defining the business operations for polygons.
//       Notice it uses DTOs, not Models - the Controller never sees Models!
//
// WHY:  The Service layer is the "brain" of the application:
//       - Validation (is this polygon valid?)
//       - Business rules (polygons must have 3+ coordinates)
//       - Data transformation (DTO ↔ Model conversion)
//
// DIFFERENCE FROM IPolygonRepository:
//       - Repository: Uses Models (Polygon), talks to database
//       - Service: Uses DTOs (PolygonDto, CreatePolygonRequest), implements logic
//
// THE LAYERING:
//       Controller → IPolygonService → IPolygonRepository → MongoDB
//       (DTOs)        (DTOs→Models)      (Models)          (BSON)
//
// See BACKEND_CONCEPTS.md: Services, Interfaces, DTOs
// ============================================================================

using MapServer.DTOs;  // For PolygonDto, CreatePolygonRequest

namespace MapServer.Services;

// ============================================================================
// IPolygonService INTERFACE
// ============================================================================
// Notice all methods use DTO types:
// - PolygonDto (response)
// - CreatePolygonRequest (request)
// - NOT Polygon (that's the Model - internal only)
//
// The Controller only knows about DTOs. It doesn't know about:
// - MongoDB
// - GeoJSON
// - BSON attributes
// - Or any other database concerns
// ============================================================================
public interface IPolygonService
{
    // ========================================================================
    // GetAllAsync - Get all polygons as DTOs
    // ========================================================================
    // Returns: List<PolygonDto> - polygons ready to send to client
    //
    // THE FLOW:
    //   1. Repository returns List<Polygon> (Models)
    //   2. Service converts each Polygon → PolygonDto
    //   3. Controller receives List<PolygonDto> (ready for JSON)
    //
    // WHY CONVERT?
    // The Model has GeoJsonPolygon (complex MongoDB type).
    // The DTO has List<Coordinate> (simple, client-friendly).
    // ========================================================================
    Task<List<PolygonDto>> GetAllAsync();

    // ========================================================================
    // GetByIdAsync - Get one polygon as a DTO
    // ========================================================================
    // Returns: PolygonDto? - the polygon or null if not found
    // The Controller decides what to do with null (return 404).
    // ========================================================================
    Task<PolygonDto?> GetByIdAsync(string id);

    // ========================================================================
    // CreateAsync - Create a new polygon from a request DTO
    // ========================================================================
    // Parameters: CreatePolygonRequest - what the client sends
    // Returns: PolygonDto - the created polygon with its new ID
    //
    // THE FLOW:
    //   1. Receives CreatePolygonRequest (list of coordinates)
    //   2. VALIDATES: At least 3 coordinates? Valid lat/long ranges?
    //   3. Ensures polygon is closed (adds first point to end if needed)
    //   4. Converts to Polygon Model with GeoJSON geometry
    //   5. Calls Repository to save
    //   6. Converts saved Polygon back to PolygonDto
    //   7. Returns PolygonDto (now with Id)
    //
    // THROWS: ArgumentException if validation fails
    // The Controller catches this and returns 400 Bad Request.
    // ========================================================================
    Task<PolygonDto> CreateAsync(CreatePolygonRequest request);

    // ========================================================================
    // DeleteAsync - Delete a polygon by ID
    // ========================================================================
    // Returns: bool - true if deleted, false if not found
    // No conversion needed - just passes through to Repository.
    // ========================================================================
    Task<bool> DeleteAsync(string id);

    // ========================================================================
    // DeleteAllAsync - Delete all polygons
    // ========================================================================
    // Just delegates to Repository. No conversion or logic needed.
    // ========================================================================
    Task DeleteAllAsync();
}
