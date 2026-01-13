using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Infrastructure.Data;
using MapServer.Infrastructure.Documents;
using MapServer.Infrastructure.Mapping;
using MongoDB.Driver;

namespace MapServer.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of IPolygonRepository.
/// Maps between domain entities and MongoDB documents.
/// </summary>
public class PolygonRepository : IPolygonRepository
{
    private readonly IMongoCollection<PolygonDocument> _polygons;

    public PolygonRepository(MongoDbContext context)
    {
        _polygons = context.Polygons;
    }

    public async Task<List<Polygon>> GetAllAsync()
    {
        var documents = await _polygons.Find(_ => true).ToListAsync();
        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<Polygon?> GetByIdAsync(string id)
    {
        var document = await _polygons.Find(p => p.Id == id).FirstOrDefaultAsync();
        return document == null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<Polygon> CreateAsync(Polygon polygon)
    {
        var document = DocumentMapper.ToDocument(polygon);
        await _polygons.InsertOneAsync(document);
        polygon.Id = document.Id;
        return polygon;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _polygons.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }
}
