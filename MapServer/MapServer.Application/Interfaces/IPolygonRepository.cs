using MapServer.Domain.Entities;

namespace MapServer.Application.Interfaces;

/// <summary>
/// Port for polygon persistence operations.
/// Implementations are in Infrastructure layer.
/// </summary>
public interface IPolygonRepository
{
    /// <summary>
    /// Retrieve all polygons.
    /// </summary>
    Task<List<Polygon>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve one polygon by its ID.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The polygon if found, null otherwise.</returns>
    Task<Polygon?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a new polygon.
    /// </summary>
    /// <param name="polygon">The polygon to save (Id will be assigned).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The polygon with its assigned Id.</returns>
    Task<Polygon> CreateAsync(Polygon polygon, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete one polygon by its ID.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
