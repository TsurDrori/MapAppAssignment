using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Application.Services;

/// <summary>
/// Orchestrates map object operations.
/// Input validation handled by DTOs via DataAnnotations.
/// </summary>
public class MapObjectService : IMapObjectService
{
    private readonly IMapObjectRepository _repository;

    public MapObjectService(IMapObjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MapObjectDto>> GetAllAsync()
    {

        var objects = await _repository.GetAllAsync();
        return objects.Select(MapToDto).ToList();
    }

    public async Task<MapObjectDto> GetByIdAsync(string id)
    {
        var obj = await _repository.GetByIdAsync(id);
        if (obj == null)
            throw new EntityNotFoundException("MapObject", id);
        return MapToDto(obj);
    }

    public async Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request)
    {
        var mapObject = new MapObject
        {
            Location = CreateGeoJsonPoint(request.Location),
            ObjectType = request.ObjectType
        };

        var created = await _repository.CreateAsync(mapObject);
        return MapToDto(created);
    }

    public async Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request)
    {
        var mapObjects = request.Objects.Select(obj => new MapObject
        {
            Location = CreateGeoJsonPoint(obj.Location),
            ObjectType = obj.ObjectType
        }).ToList();

        var created = await _repository.CreateManyAsync(mapObjects);
        return created.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new EntityNotFoundException("MapObject", id);
    }

    #region Private Helpers

    private static GeoJsonPoint<GeoJson2DGeographicCoordinates> CreateGeoJsonPoint(Coordinate coord)
    {
        return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(coord.Longitude, coord.Latitude)
        );
    }

    private static MapObjectDto MapToDto(MapObject obj)
    {
        return new MapObjectDto
        {
            Id = obj.Id,
            Location = new Coordinate
            {
                Latitude = obj.Location.Coordinates.Latitude,
                Longitude = obj.Location.Coordinates.Longitude
            },
            ObjectType = obj.ObjectType
        };
    }

    #endregion
}
