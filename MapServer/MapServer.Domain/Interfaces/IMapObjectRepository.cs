using MapServer.Domain.Entities;

namespace MapServer.Domain.Interfaces;

/// <summary>
/// Contract for map object data access operations.
/// Includes batch creation support for efficient bulk inserts.
/// </summary>
public interface IMapObjectRepository
{
    /// <summary>
    /// Retrieve all map objects from the database.
    /// </summary>
    Task<List<MapObject>> GetAllAsync();

    /// <summary>
    /// Retrieve one map object by its ID.
    /// </summary>
    /// <param name="id">The MongoDB ObjectId as a string.</param>
    /// <returns>The map object if found, null otherwise.</returns>
    Task<MapObject?> GetByIdAsync(string id);

    /// <summary>
    /// Save a single new map object to the database.
    /// </summary>
    /// <param name="mapObject">The map object to save.</param>
    /// <returns>The map object with its assigned Id.</returns>
    Task<MapObject> CreateAsync(MapObject mapObject);

    /// <summary>
    /// Save multiple map objects at once (batch operation).
    /// More efficient than individual inserts - single database round trip.
    /// </summary>
    /// <param name="mapObjects">The list of map objects to save.</param>
    /// <returns>The map objects with their assigned Ids.</returns>
    Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects);

    /// <summary>
    /// Delete one map object by its ID.
    /// </summary>
    /// <param name="id">The MongoDB ObjectId as a string.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Delete all map objects from the database.
    /// </summary>
    Task DeleteAllAsync();
}
