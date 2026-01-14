using MapServer.Application.DTOs;

namespace MapServer.Application.Interfaces;

/// <summary>
/// Contract for polygon business operations.
/// Uses DTOs for input/output - callers never see domain entities.
/// </summary>
public interface IPolygonService
{
    /// <summary>
    /// Get all polygons.
    /// </summary>
    Task<List<PolygonDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get one polygon by its ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if polygon not found.</exception>
    Task<PolygonDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new polygon.
    /// </summary>
    /// <exception cref="InvalidGeometryException">Thrown if geometry is invalid.</exception>
    Task<PolygonDto> CreateAsync(CreatePolygonRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a polygon by ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if polygon not found.</exception>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
