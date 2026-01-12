using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.Interfaces;
using MapServer.Infrastructure.Data;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IMapObjectRepository.
/// Handles all database operations for map objects, including batch insertion.
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
        return await _mapObjects.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<MapObject> CreateAsync(MapObject mapObject)
    {
        try
        {
            await _mapObjects.InsertOneAsync(mapObject);
            return mapObject;
        }
        catch (MongoWriteException ex) when (IsInvalidGeometryError(ex))
        {
            throw new InvalidGeometryException(
                "Map object location is invalid.",
                ex);
        }
    }

    public async Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects)
    {
        try
        {
            await _mapObjects.InsertManyAsync(mapObjects);
            return mapObjects;
        }
        catch (MongoBulkWriteException ex) when (IsInvalidGeometryError(ex))
        {
            throw new InvalidGeometryException(
                "One or more map object locations are invalid.",
                ex);
        }
    }

    private static bool IsInvalidGeometryError(MongoWriteException ex)
    {
        if (ex.WriteError?.Code == 16755)
            return true;

        var message = ex.WriteError?.Message ?? string.Empty;
        return message.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInvalidGeometryError(MongoBulkWriteException ex)
    {
        return ex.WriteErrors.Any(e =>
            e.Code == 16755 ||
            (e.Message?.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase) ?? false));
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _mapObjects.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task DeleteAllAsync()
    {
        await _mapObjects.DeleteManyAsync(_ => true);
    }
}
