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

    public async Task<List<MapObject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _mapObjects.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<MapObject?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _mapObjects.Find(o => o.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document == null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<MapObject> CreateAsync(MapObject mapObject, CancellationToken cancellationToken = default)
    {
        var document = DocumentMapper.ToDocument(mapObject);
        await _mapObjects.InsertOneAsync(document, options: null, cancellationToken);
        mapObject.Id = document.Id;
        return mapObject;
    }

    public async Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects, CancellationToken cancellationToken = default)
    {
        var documents = mapObjects.Select(DocumentMapper.ToDocument).ToList();
        await _mapObjects.InsertManyAsync(documents, options: null, cancellationToken);

        for (int i = 0; i < mapObjects.Count; i++)
        {
            mapObjects[i].Id = documents[i].Id;
        }
        return mapObjects;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _mapObjects.DeleteOneAsync(o => o.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
