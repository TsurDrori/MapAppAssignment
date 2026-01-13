using MapServer.Domain.Entities;

namespace MapServer.Domain.Interfaces;

/// <summary>
/// Contract for polygon data access operations.
/// Implementations handle the actual database interaction.
/// </summary>
public interface IPolygonRepository
{
    /// <summary>
    /// Retrieve all polygons from the database.
    /// </summary>
    Task<List<Polygon>> GetAllAsync();

    /// <summary>
    /// Retrieve one polygon by its ID.
    /// </summary>
    /// <param name="id">The MongoDB ObjectId as a string.</param>
    /// <returns>The polygon if found, null otherwise.</returns>
    Task<Polygon?> GetByIdAsync(string id);

    /// <summary>
    /// Save a new polygon to the database.
    /// </summary>
    /// <param name="polygon">The polygon to save (Id will be assigned by MongoDB).</param>
    /// <returns>The polygon with its assigned Id.</returns>
    Task<Polygon> CreateAsync(Polygon polygon);

    /// <summary>
    /// Delete one polygon by its ID.
    /// </summary>
    /// <param name="id">The MongoDB ObjectId as a string.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id);
}
