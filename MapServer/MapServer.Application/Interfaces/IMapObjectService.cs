using MapServer.Application.DTOs;

namespace MapServer.Application.Interfaces;

/// <summary>
/// Contract for map object business operations.
/// Includes batch creation for efficient bulk operations.
/// </summary>
public interface IMapObjectService
{
    /// <summary>
    /// Get all map objects.
    /// </summary>
    Task<List<MapObjectDto>> GetAllAsync();

    /// <summary>
    /// Get one map object by its ID.
    /// </summary>
    /// <returns>The map object or null if not found.</returns>
    Task<MapObjectDto?> GetByIdAsync(string id);

    /// <summary>
    /// Create a single map object.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request);

    /// <summary>
    /// Create multiple map objects in one batch operation.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if any object fails validation.</exception>
    Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request);

    /// <summary>
    /// Delete a map object by ID.
    /// </summary>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Delete all map objects.
    /// </summary>
    Task DeleteAllAsync();
}
