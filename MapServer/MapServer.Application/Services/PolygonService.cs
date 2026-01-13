using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Interfaces;
using MapServer.Domain.ValueObjects;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Application.Services;

/// <summary>
/// Orchestrates polygon operations.
/// Delegates validation to DTOs (input) and Domain (business rules).
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

    public async Task<PolygonDto?> GetByIdAsync(string id)
    {
        var polygon = await _repository.GetByIdAsync(id);
        return polygon == null ? null : MapToDto(polygon);
    }

    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request)
    {
        // Domain validates geometry rules (closure, self-intersection)
        var ring = LinearRing.Create(
            request.Coordinates.Select(c => (c.Longitude, c.Latitude))
        );

        var polygon = new Polygon
        {
            Geometry = CreateGeoJsonPolygon(ring)
        };

        var created = await _repository.CreateAsync(polygon);
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task DeleteAllAsync()
    {
        await _repository.DeleteAllAsync();
    }

    #region Private Helpers

    private static GeoJsonPolygon<GeoJson2DGeographicCoordinates> CreateGeoJsonPolygon(LinearRing ring)
    {
        var geoJsonCoordinates = ring.Coordinates
            .Select(c => new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude))
            .ToList();

        var linearRing = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(geoJsonCoordinates);

        return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRing)
        );
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
