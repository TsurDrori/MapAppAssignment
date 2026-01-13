using MapServer.Domain.Entities;
using MapServer.Domain.Interfaces;
using MapServer.Infrastructure.Data;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IPolygonRepository.
/// Pure data access - no exception translation.
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
        await _polygons.InsertOneAsync(polygon);
        return polygon;
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
