// ============================================================================
// IMapObjectRepository.cs - THE CONTRACT FOR MAP OBJECT DATA ACCESS
// ============================================================================
//
// WHAT: Interface defining operations for map objects (markers, vehicles, etc.).
//       Similar to IPolygonRepository, but adds batch creation support.
//
// WHY:  Same benefits as IPolygonRepository:
//       - Swappable implementations (real vs. mock)
//       - Clean contract for services
//       - Enables dependency injection
//
// DIFFERENCE FROM IPolygonRepository:
//       Has CreateManyAsync for batch insertion (per assignment requirements).
//
// See BACKEND_CONCEPTS.md: Interfaces, Repositories, Dependency Injection
// ============================================================================

using MapServer.Models;  // For MapObject type

namespace MapServer.Repositories;

// ============================================================================
// IMapObjectRepository INTERFACE
// ============================================================================
// Defines 6 operations:
// - GetAllAsync: Get all map objects
// - GetByIdAsync: Get one by ID
// - CreateAsync: Create one object
// - CreateManyAsync: Create multiple objects at once (BATCH)
// - DeleteAsync: Delete one by ID
// - DeleteAllAsync: Delete all objects
// ============================================================================
public interface IMapObjectRepository
{
    // ========================================================================
    // GetAllAsync - Retrieve all map objects from the database
    // ========================================================================
    // Same pattern as IPolygonRepository.GetAllAsync
    // Returns all map objects for displaying on the map.
    // ========================================================================
    Task<List<MapObject>> GetAllAsync();

    // ========================================================================
    // GetByIdAsync - Retrieve one map object by its ID
    // ========================================================================
    // Returns MapObject? (nullable) - might not exist.
    // Same pattern as IPolygonRepository.GetByIdAsync
    // ========================================================================
    Task<MapObject?> GetByIdAsync(string id);

    // ========================================================================
    // CreateAsync - Save a single new map object
    // ========================================================================
    // Parameters: mapObject with Location and ObjectType set
    // Returns: The same object with Id now assigned by MongoDB
    // ========================================================================
    Task<MapObject> CreateAsync(MapObject mapObject);

    // ========================================================================
    // CreateManyAsync - Save multiple map objects at once (BATCH OPERATION)
    // ========================================================================
    // Parameters:
    //   - mapObjects: List of MapObject to save
    //
    // Returns:
    //   - List<MapObject>: Same objects, but now with Ids assigned
    //
    // WHY BATCH?
    // Creating 100 objects individually = 100 database round trips.
    // Creating 100 objects in a batch = 1 database round trip.
    // MUCH faster for bulk operations.
    //
    // MONGODB DRIVER:
    // Uses InsertManyAsync instead of InsertOneAsync.
    // More efficient because:
    // - Single network call
    // - Single write operation in the database
    // - All documents get Ids assigned atomically
    //
    // USE CASE:
    // POST /api/objects/batch with an array of objects.
    // The assignment specifically requires this endpoint.
    // ========================================================================
    Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects);

    // ========================================================================
    // DeleteAsync - Delete one map object by its ID
    // ========================================================================
    // Returns: true if deleted, false if not found
    // Same pattern as IPolygonRepository.DeleteAsync
    // ========================================================================
    Task<bool> DeleteAsync(string id);

    // ========================================================================
    // DeleteAllAsync - Delete ALL map objects
    // ========================================================================
    // Clears the entire collection.
    // Same pattern as IPolygonRepository.DeleteAllAsync
    // ========================================================================
    Task DeleteAllAsync();
}
