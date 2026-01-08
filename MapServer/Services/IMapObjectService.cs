// ============================================================================
// IMapObjectService.cs - THE CONTRACT FOR MAP OBJECT BUSINESS LOGIC
// ============================================================================
//
// WHAT: Interface defining business operations for map objects.
//       Similar to IPolygonService, but adds batch creation support.
//
// WHY:  The Service layer handles:
//       - Validation (coordinate ranges)
//       - Data transformation (Coordinate → GeoJsonPoint)
//       - Business rules (no ObjectType validation - client decides)
//
// See BACKEND_CONCEPTS.md: Services, Interfaces
// ============================================================================

using MapServer.DTOs;  // For MapObjectDto, CreateMapObjectRequest, BatchCreateMapObjectsRequest

namespace MapServer.Services;

// ============================================================================
// IMapObjectService INTERFACE
// ============================================================================
// All methods use DTOs - the Controller never touches Models.
// ============================================================================
public interface IMapObjectService
{
    // ========================================================================
    // GetAllAsync - Get all map objects as DTOs
    // ========================================================================
    // Returns: List<MapObjectDto> - all objects ready for JSON serialization
    // ========================================================================
    Task<List<MapObjectDto>> GetAllAsync();

    // ========================================================================
    // GetByIdAsync - Get one map object as a DTO
    // ========================================================================
    // Returns: MapObjectDto? - the object or null if not found
    // ========================================================================
    Task<MapObjectDto?> GetByIdAsync(string id);

    // ========================================================================
    // CreateAsync - Create a single map object
    // ========================================================================
    // Parameters: CreateMapObjectRequest (Location + ObjectType)
    // Returns: MapObjectDto with the new ID
    // THROWS: ArgumentException if Location is invalid
    // ========================================================================
    Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request);

    // ========================================================================
    // CreateManyAsync - Create multiple map objects at once (BATCH)
    // ========================================================================
    // Parameters: BatchCreateMapObjectsRequest (list of objects to create)
    // Returns: List<MapObjectDto> - all created objects with their IDs
    // THROWS: ArgumentException if ANY Location is invalid
    //
    // WHY BATCH?
    // Creating 100 objects individually = slow (100 HTTP calls, 100 DB calls)
    // Creating 100 objects in batch = fast (1 HTTP call, 1 DB call)
    //
    // The assignment specifically requires this endpoint:
    // POST /api/objects/batch
    // ========================================================================
    Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request);

    // ========================================================================
    // DeleteAsync - Delete one map object by ID
    // ========================================================================
    // Returns: bool - true if deleted, false if not found
    // ========================================================================
    Task<bool> DeleteAsync(string id);

    // ========================================================================
    // DeleteAllAsync - Delete all map objects
    // ========================================================================
    Task DeleteAllAsync();
}
