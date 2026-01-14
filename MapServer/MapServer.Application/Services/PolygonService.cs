using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.ValueObjects;

namespace MapServer.Application.Services;

/// <summary>
/// Orchestrates polygon operations.
/// Input validation handled by DTOs, geometry validation by MongoDB.
/// </summary>
public class PolygonService : IPolygonService
{
    private readonly IPolygonRepository _repository;

    public PolygonService(IPolygonRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PolygonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var polygons = await _repository.GetAllAsync(cancellationToken);
        return polygons.Select(MapToDto).ToList();
    }

    public async Task<PolygonDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var polygon = await _repository.GetByIdAsync(id, cancellationToken);
        if (polygon == null)
            throw new EntityNotFoundException("Polygon", id);
        return MapToDto(polygon);
    }

    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request, CancellationToken cancellationToken = default)
    {
        // Guard validation - consistent behavior regardless of entry point
        if (request.Coordinates == null || request.Coordinates.Count < 3)
            throw new ValidationException("Polygon must have at least 3 coordinates");

        var coordinates = EnsureClosed(request.Coordinates);

        var polygon = new Polygon
        {
            Coordinates = coordinates
                .Select(c => new GeoCoordinate(c.Latitude, c.Longitude))
                .ToList()
        };

        var created = await _repository.CreateAsync(polygon, cancellationToken);
        return MapToDto(created);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new EntityNotFoundException("Polygon", id);
    }

    #region Private Helpers

    private static List<Coordinate> EnsureClosed(List<Coordinate> coordinates)
    {
        var first = coordinates[0];
        var last = coordinates[^1];

        if (first.Latitude == last.Latitude && first.Longitude == last.Longitude)
            return coordinates;

        return [.. coordinates, first];
    }

    private static PolygonDto MapToDto(Polygon polygon)
    {
        var coordinates = polygon.Coordinates
            .Select(c => new Coordinate
            {
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToList();

        return new PolygonDto
        {
            Id = polygon.Id,
            Coordinates = coordinates
        };
    }

    #endregion
}
