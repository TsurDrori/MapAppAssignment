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
    Task<List<MapObjectDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get one map object by its ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if object not found.</exception>
    Task<MapObjectDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a single map object.
    /// </summary>
    Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create multiple map objects in one batch operation.
    /// </summary>
    Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a map object by ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if object not found.</exception>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
