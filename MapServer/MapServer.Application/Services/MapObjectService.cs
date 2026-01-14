using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Exceptions;
using MapServer.Domain.ValueObjects;

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
        // Guard validation - consistent behavior regardless of entry point
        ValidateRequest(request);

        var mapObject = new MapObject
        {
            Location = new GeoCoordinate(request.Location.Latitude, request.Location.Longitude),
            ObjectType = request.ObjectType
        };

        var created = await _repository.CreateAsync(mapObject);
        return MapToDto(created);
    }

    public async Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request)
    {
        // Guard validation
        if (request.Objects == null || request.Objects.Count == 0)
            throw new ValidationException("At least one object is required");

        foreach (var obj in request.Objects)
        {
            ValidateRequest(obj);
        }

        var mapObjects = request.Objects.Select(obj => new MapObject
        {
            Location = new GeoCoordinate(obj.Location.Latitude, obj.Location.Longitude),
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

    private static void ValidateRequest(CreateMapObjectRequest request)
    {
        if (request.Location == null)
            throw new ValidationException("Location is required");

        if (string.IsNullOrWhiteSpace(request.ObjectType))
            throw new ValidationException("ObjectType is required");
    }

    private static MapObjectDto MapToDto(MapObject obj)
    {
        return new MapObjectDto
        {
            Id = obj.Id,
            Location = new Coordinate
            {
                Latitude = obj.Location.Latitude,
                Longitude = obj.Location.Longitude
            },
            ObjectType = obj.ObjectType
        };
    }

    #endregion
}
