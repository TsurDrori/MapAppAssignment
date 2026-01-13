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
    Task<List<PolygonDto>> GetAllAsync();

    /// <summary>
    /// Get one polygon by its ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if polygon not found.</exception>
    Task<PolygonDto> GetByIdAsync(string id);

    /// <summary>
    /// Create a new polygon.
    /// </summary>
    /// <exception cref="InvalidGeometryException">Thrown if geometry is invalid.</exception>
    Task<PolygonDto> CreateAsync(CreatePolygonRequest request);

    /// <summary>
    /// Delete a polygon by ID.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if polygon not found.</exception>
    Task DeleteAsync(string id);
}
