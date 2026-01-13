using MapServer.Domain.Entities;
using MapServer.Domain.Interfaces;
using MapServer.Infrastructure.Data;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IMapObjectRepository.
/// Pure data access - no exception translation.
/// </summary>
public class MapObjectRepository : IMapObjectRepository
{
    private readonly IMongoCollection<MapObject> _mapObjects;

    public MapObjectRepository(MongoDbContext context)
    {
        _mapObjects = context.MapObjects;
    }

    public async Task<List<MapObject>> GetAllAsync()
    {
        return await _mapObjects.Find(_ => true).ToListAsync();
    }

    public async Task<MapObject?> GetByIdAsync(string id)
    {
        var res = await _mapObjects.Find(o => o.Id == id).FirstOrDefaultAsync();//check if this is the right function for o
        return res;
    }

    public async Task<MapObject> CreateAsync(MapObject mapObject)
    {
        await _mapObjects.InsertOneAsync(mapObject);
        return mapObject;
    }

    public async Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects)
    {
        await _mapObjects.InsertManyAsync(mapObjects);
        return mapObjects;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _mapObjects.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount > 0;
    }
}
