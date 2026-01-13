using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Infrastructure.Data;
using MapServer.Infrastructure.Documents;
using MapServer.Infrastructure.Mapping;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IMapObjectRepository.
/// Maps between domain entities and MongoDB documents.
/// </summary>
public class MapObjectRepository : IMapObjectRepository
{
    private readonly IMongoCollection<MapObjectDocument> _mapObjects;

    public MapObjectRepository(MongoDbContext context)
    {
        _mapObjects = context.MapObjects;
    }

    public async Task<List<MapObject>> GetAllAsync()
    {
        var documents = await _mapObjects.Find(_ => true).ToListAsync();
        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<MapObject?> GetByIdAsync(string id)
    {
        var document = await _mapObjects.Find(o => o.Id == id).FirstOrDefaultAsync();
        return document == null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<MapObject> CreateAsync(MapObject mapObject)
    {
        var document = DocumentMapper.ToDocument(mapObject);
        await _mapObjects.InsertOneAsync(document);
        mapObject.Id = document.Id;
        return mapObject;
    }

    public async Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects)
    {
        var documents = mapObjects.Select(DocumentMapper.ToDocument).ToList();
        await _mapObjects.InsertManyAsync(documents);

        for (int i = 0; i < mapObjects.Count; i++)
        {
            mapObjects[i].Id = documents[i].Id;
        }
        return mapObjects;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _mapObjects.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount > 0;
    }
}
