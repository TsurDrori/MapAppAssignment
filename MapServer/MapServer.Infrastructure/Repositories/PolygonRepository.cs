using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.Interfaces;
using MapServer.Infrastructure.Data;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IPolygonRepository.
/// Handles all database operations for polygons.
/// </summary>
public class PolygonRepository : IPolygonRepository
{
    private readonly IMongoCollection<Polygon> _polygons;

    public PolygonRepository(MongoDbContext context)
    {
        _polygons = context.Polygons;
    }

    public async Task<List<Polygon>> GetAllAsync()
    {
        return await _polygons.Find(_ => true).ToListAsync();
    }

    public async Task<Polygon?> GetByIdAsync(string id)
    {
        return await _polygons.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Polygon> CreateAsync(Polygon polygon)
    {
        try
        {
            await _polygons.InsertOneAsync(polygon);
            return polygon;
        }
        catch (MongoWriteException ex) when (IsInvalidGeometryError(ex))
        {
            throw new InvalidGeometryException(
                "Polygon geometry is invalid. Edges must not cross each other (self-intersection).",
                ex);
        }
    }

    private static bool IsInvalidGeometryError(MongoWriteException ex)
    {
        // MongoDB error code 16755: Can't extract geo keys (invalid GeoJSON)
        if (ex.WriteError?.Code == 16755)
            return true;

        var message = ex.WriteError?.Message ?? string.Empty;
        return message.Contains("Loop is not valid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _polygons.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task DeleteAllAsync()
    {
        await _polygons.DeleteManyAsync(_ => true);
    }
}
