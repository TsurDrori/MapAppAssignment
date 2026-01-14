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

    public async Task<List<Polygon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _polygons.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<Polygon?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _polygons.Find(p => p.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document == null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<Polygon> CreateAsync(Polygon polygon, CancellationToken cancellationToken = default)
    {
        var document = DocumentMapper.ToDocument(polygon);
        await _polygons.InsertOneAsync(document, options: null, cancellationToken);
        polygon.Id = document.Id;
        return polygon;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _polygons.DeleteOneAsync(p => p.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
