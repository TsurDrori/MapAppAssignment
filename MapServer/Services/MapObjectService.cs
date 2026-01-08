// ============================================================================
// MapObjectService.cs - THE BUSINESS LOGIC FOR MAP OBJECTS
// ============================================================================
//
// WHAT: Handles all business logic for map objects (markers, vehicles, etc.):
//       - Validates coordinates
//       - Converts between DTOs and Models
//       - Supports single and batch creation
//
// WHY:  Same pattern as PolygonService - separation of concerns.
//       Controller handles HTTP, Repository handles MongoDB, Service handles logic.
//
// SIMPLER THAN PolygonService:
//       - Points are simpler than polygons (just one coordinate)
//       - No "closing" logic needed
//       - ObjectType has no validation (any string accepted)
//
// See BACKEND_CONCEPTS.md: Services, Lambda Expressions
// ============================================================================

using MapServer.DTOs;
using MapServer.Models;
using MapServer.Repositories;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Services;

// ============================================================================
// MapObjectService CLASS
// ============================================================================
public class MapObjectService : IMapObjectService
{
    // ========================================================================
    // PRIVATE FIELD - The repository for database operations
    // ========================================================================
    private readonly IMapObjectRepository _repository;

    // ========================================================================
    // CONSTRUCTOR - Receives repository via dependency injection
    // ========================================================================
    public MapObjectService(IMapObjectRepository repository)
    {
        _repository = repository;
    }

    // ========================================================================
    // GetAllAsync - Get all map objects and convert to DTOs
    // ========================================================================
    // Same pattern as PolygonService:
    // 1. Get Models from repository
    // 2. Convert each to DTO using MapToDto
    // 3. Return as list
    // ========================================================================
    public async Task<List<MapObjectDto>> GetAllAsync()
    {
        var objects = await _repository.GetAllAsync();
        return objects.Select(MapToDto).ToList();
    }

    // ========================================================================
    // GetByIdAsync - Get one map object and convert to DTO
    // ========================================================================
    // Returns null if not found. Controller handles 404 response.
    // ========================================================================
    public async Task<MapObjectDto?> GetByIdAsync(string id)
    {
        var obj = await _repository.GetByIdAsync(id);
        return obj == null ? null : MapToDto(obj);
    }

    // ========================================================================
    // CreateAsync - Create a single map object
    // ========================================================================
    // THE FLOW:
    //   1. Validate the coordinate (must be in valid lat/long range)
    //   2. Create MapObject Model with GeoJSON point
    //   3. Save to database
    //   4. Convert result to DTO and return
    //
    // THROWS: ArgumentException if coordinate is invalid
    // ========================================================================
    public async Task<MapObjectDto> CreateAsync(CreateMapObjectRequest request)
    {
        // Validate coordinate - throws if invalid
        ValidateCoordinate(request.Location);

        // Create the Model with GeoJSON point and ObjectType
        var mapObject = new MapObject
        {
            Location = CreateGeoJsonPoint(request.Location),  // Convert to GeoJSON
            ObjectType = request.ObjectType                   // Just copy - no validation
        };

        // Save and get result with ID
        var created = await _repository.CreateAsync(mapObject);

        // Convert to DTO and return
        return MapToDto(created);
    }

    // ========================================================================
    // CreateManyAsync - Create multiple map objects in one batch
    // ========================================================================
    // THE FLOW:
    //   1. For each object in request:
    //      - Validate its coordinate
    //      - Create MapObject Model with GeoJSON point
    //   2. Save ALL objects in one repository call (efficient!)
    //   3. Convert all results to DTOs
    //
    // WHY THIS IS FAST:
    //   - One HTTP request instead of many
    //   - One database call (InsertManyAsync) instead of many
    //   - Much less overhead per object
    //
    // THROWS: ArgumentException if ANY coordinate is invalid
    //   - All validation happens BEFORE any database write
    //   - If one fails, nothing gets saved
    // ========================================================================
    public async Task<List<MapObjectDto>> CreateManyAsync(BatchCreateMapObjectsRequest request)
    {
        // Convert all request objects to Models
        // This lambda is more complex - it has a body with multiple statements
        var mapObjects = request.Objects.Select(obj =>
        {
            // Validate each coordinate (throws if invalid)
            ValidateCoordinate(obj.Location);

            // Create and return the Model
            return new MapObject
            {
                Location = CreateGeoJsonPoint(obj.Location),
                ObjectType = obj.ObjectType
            };
        }).ToList();  // Execute the transformation and collect results

        // Save all objects in one database call
        var created = await _repository.CreateManyAsync(mapObjects);

        // Convert all results to DTOs and return
        return created.Select(MapToDto).ToList();
    }

    // ========================================================================
    // DeleteAsync - Just delegate to repository
    // ========================================================================
    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    // ========================================================================
    // DeleteAllAsync - Just delegate to repository
    // ========================================================================
    public async Task DeleteAllAsync()
    {
        await _repository.DeleteAllAsync();
    }

    // ============================================================================
    // PRIVATE HELPER METHODS
    // ============================================================================

    // ========================================================================
    // ValidateCoordinate - Check if coordinate is in valid range
    // ========================================================================
    // THROWS: ArgumentException if invalid
    //
    // Same rules as PolygonService:
    // - Latitude: -90 to 90
    // - Longitude: -180 to 180
    // ========================================================================
    private static void ValidateCoordinate(Coordinate coord)
    {
        if (coord.Latitude < -90 || coord.Latitude > 90)
        {
            throw new ArgumentException($"Latitude must be between -90 and 90, got {coord.Latitude}");
        }

        if (coord.Longitude < -180 || coord.Longitude > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {coord.Longitude}");
        }
    }

    // ========================================================================
    // CreateGeoJsonPoint - Convert simple Coordinate to MongoDB GeoJSON Point
    // ========================================================================
    // A GeoJsonPoint is simpler than GeoJsonPolygon - just one coordinate.
    //
    // COORDINATE ORDER:
    // Our Coordinate: (Latitude, Longitude)
    // GeoJSON standard: (Longitude, Latitude)
    // We swap the order during conversion!
    // ========================================================================
    private static GeoJsonPoint<GeoJson2DGeographicCoordinates> CreateGeoJsonPoint(Coordinate coord)
    {
        // Create GeoJSON point with (Longitude, Latitude) order
        return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(coord.Longitude, coord.Latitude)  // Longitude first!
        );
    }

    // ========================================================================
    // MapToDto - Convert MapObject Model to MapObjectDto
    // ========================================================================
    // Extracts data from the GeoJSON structure and creates a simple DTO.
    //
    // PROPERTY CHAIN:
    //   obj.Location              = the GeoJsonPoint
    //      .Coordinates           = the GeoJson2DGeographicCoordinates
    //      .Latitude/.Longitude   = the actual values
    // ========================================================================
    private static MapObjectDto MapToDto(MapObject obj)
    {
        return new MapObjectDto
        {
            // Copy the ID
            Id = obj.Id,

            // Extract location from GeoJSON and convert to our Coordinate format
            Location = new Coordinate
            {
                Latitude = obj.Location.Coordinates.Latitude,
                Longitude = obj.Location.Coordinates.Longitude
            },

            // Copy the object type as-is
            ObjectType = obj.ObjectType
        };
    }
}
