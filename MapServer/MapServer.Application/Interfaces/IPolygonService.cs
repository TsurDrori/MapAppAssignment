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
    /// <returns>The polygon or null if not found.</returns>
    Task<PolygonDto?> GetByIdAsync(string id);

    /// <summary>
    /// Create a new polygon.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    Task<PolygonDto> CreateAsync(CreatePolygonRequest request);

    /// <summary>
    /// Delete a polygon by ID.
    /// </summary>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Delete all polygons.
    /// </summary>
    Task DeleteAllAsync();
}
