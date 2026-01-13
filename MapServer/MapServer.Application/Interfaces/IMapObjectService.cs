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
    /// <exception cref="EntityNotFoundException">Thrown if object not found.</exception>
    Task<MapObjectDto> GetByIdAsync(string id);

    /// <summary>
    /// Create a single map object.
    /// </summary>
    Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request);

    /// <summary>
    /// Create multiple map objects in one batch operation.
    /// </summary>
    Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request);

    /// <summary>
    /// Delete a map object by ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if object not found.</exception>
    Task DeleteAsync(string id);

    /// <summary>
    /// Delete all map objects.
    /// </summary>
    Task DeleteAllAsync();
}
