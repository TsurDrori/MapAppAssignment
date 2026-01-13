using MapServer.Domain.Entities;

namespace MapServer.Application.Interfaces;

/// <summary>
/// Port for map object persistence operations.
/// Implementations are in Infrastructure layer.
/// </summary>
public interface IMapObjectRepository
{
    /// <summary>
    /// Retrieve all map objects.
    /// </summary>
    Task<List<MapObject>> GetAllAsync();

    /// <summary>
    /// Retrieve one map object by its ID.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>The map object if found, null otherwise.</returns>
    Task<MapObject?> GetByIdAsync(string id);

    /// <summary>
    /// Save a single new map object.
    /// </summary>
    /// <param name="mapObject">The map object to save.</param>
    /// <returns>The map object with its assigned Id.</returns>
    Task<MapObject> CreateAsync(MapObject mapObject);

    /// <summary>
    /// Save multiple map objects at once (batch operation).
    /// </summary>
    /// <param name="mapObjects">The list of map objects to save.</param>
    /// <returns>The map objects with their assigned Ids.</returns>
    Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects);

    /// <summary>
    /// Delete one map object by its ID.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id);
}
