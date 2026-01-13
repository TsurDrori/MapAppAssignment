using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.Interfaces;
using MongoDB.Driver.GeoJsonObjectModel;

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

    public async Task<List<PolygonDto>> GetAllAsync()
    {
        var polygons = await _repository.GetAllAsync();
        return polygons.Select(MapToDto).ToList();
    }

    public async Task<PolygonDto> GetByIdAsync(string id)
    {
        var polygon = await _repository.GetByIdAsync(id);
        if (polygon == null)
            throw new EntityNotFoundException("Polygon", id);
        return MapToDto(polygon);
    }

    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request)
    {
        var polygon = new Polygon
        {
            Geometry = CreateGeoJsonPolygon(request.Coordinates)
        };

        var created = await _repository.CreateAsync(polygon);
        return MapToDto(created);
    }

    public async Task DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new EntityNotFoundException("Polygon", id);
    }

    public async Task DeleteAllAsync()
    {
        await _repository.DeleteAllAsync();
    }

    #region Private Helpers

    private static GeoJsonPolygon<GeoJson2DGeographicCoordinates> CreateGeoJsonPolygon(List<Coordinate> coordinates)
    {
        var closed = EnsureClosed(coordinates);

        var geoJsonCoordinates = closed
            .Select(c => new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude))
            .ToList();

        var linearRing = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(geoJsonCoordinates);

        return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRing)
        );
    }

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
        var coordinates = polygon.Geometry.Coordinates.Exterior.Positions
            .Select(pos => new Coordinate
            {
                Latitude = pos.Latitude,
                Longitude = pos.Longitude
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
