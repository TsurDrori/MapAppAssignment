using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.Interfaces;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Application.Services;

/// <summary>
/// Business logic for map object operations.
/// Handles validation, coordinate transformation, and batch operations.
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

    public async Task<MapObjectDto?> GetByIdAsync(string id)
    {
        var obj = await _repository.GetByIdAsync(id);
        return obj == null ? null : MapToDto(obj);
    }

    public async Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request)
    {
        ValidateCoordinate(request.Location);

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
        var mapObjects = request.Objects.Select(obj =>
        {
            ValidateCoordinate(obj.Location);

            return new MapObject
            {
                Location = CreateGeoJsonPoint(obj.Location),
                ObjectType = obj.ObjectType
            };
        }).ToList();

        var created = await _repository.CreateManyAsync(mapObjects);
        return created.Select(MapToDto).ToList();
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

    private static void ValidateCoordinate(Coordinate coord)
    {
        if (coord.Latitude < -90 || coord.Latitude > 90)
        {
            throw new ValidationException($"Latitude must be between -90 and 90, got {coord.Latitude}");
        }

        if (coord.Longitude < -180 || coord.Longitude > 180)
        {
            throw new ValidationException($"Longitude must be between -180 and 180, got {coord.Longitude}");
        }
    }

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
