// ============================================================================
// PolygonService.cs - THE BUSINESS LOGIC FOR POLYGONS
// ============================================================================
//
// WHAT: This is where the "business logic" lives. It:
//       - Validates input (at least 3 coordinates, valid ranges)
//       - Transforms data (DTO ↔ Model, simple coords ↔ GeoJSON)
//       - Enforces rules (auto-close polygons)
//       - Orchestrates between Controller and Repository
//
// WHY:  Separation of concerns. The Controller handles HTTP, the Repository
//       handles MongoDB. The Service handles EVERYTHING ELSE:
//       - "Is this data valid?"
//       - "How do I convert between formats?"
//       - "What are the business rules?"
//
// See BACKEND_CONCEPTS.md: Services, Lambda Expressions
// ============================================================================

using MapServer.DTOs;                        // For PolygonDto, CreatePolygonRequest, Coordinate
using MapServer.Models;                      // For Polygon (the MongoDB model)
using MapServer.Repositories;                // For IPolygonRepository
using MongoDB.Driver.GeoJsonObjectModel;     // For GeoJsonPolygon and related types

namespace MapServer.Services;

// ============================================================================
// PolygonService CLASS
// ============================================================================
// ": IPolygonService" = implements the interface
// This class contains all the business logic for polygon operations.
// ============================================================================
public class PolygonService : IPolygonService
{
    // ========================================================================
    // PRIVATE FIELD - The repository we'll use for database operations
    // ========================================================================
    // We depend on the INTERFACE (IPolygonRepository), not the concrete class.
    // This makes the service testable - in tests, we can inject a mock.
    // ========================================================================
    private readonly IPolygonRepository _repository;

    // ========================================================================
    // CONSTRUCTOR - Receives the repository via dependency injection
    // ========================================================================
    // ASP.NET Core's DI container:
    // 1. Sees PolygonService needs IPolygonRepository
    // 2. Looks up what was registered for IPolygonRepository
    // 3. Creates a PolygonRepository and passes it here
    //
    // See BACKEND_CONCEPTS.md: Dependency Injection
    // ========================================================================
    public PolygonService(IPolygonRepository repository)
    {
        _repository = repository;
    }

    // ========================================================================
    // GetAllAsync - Get all polygons and convert to DTOs
    // ========================================================================
    // THE FLOW:
    //   1. Call repository to get all Polygon models from database
    //   2. Convert each Polygon to PolygonDto using MapToDto
    //   3. Return the list of DTOs
    //
    // LINQ EXPLAINED:
    //   polygons.Select(MapToDto).ToList()
    //   - .Select(MapToDto) = "for each polygon, call MapToDto and collect results"
    //   - .ToList() = "convert the result to a List<PolygonDto>"
    //
    // This is like:
    //   var result = new List<PolygonDto>();
    //   foreach (var p in polygons) {
    //       result.Add(MapToDto(p));
    //   }
    //   return result;
    //
    // See BACKEND_CONCEPTS.md: Lambda Expressions
    // ========================================================================
    public async Task<List<PolygonDto>> GetAllAsync()
    {
        // Get all Polygon models from the database
        var polygons = await _repository.GetAllAsync();

        // Convert each Polygon to PolygonDto and return as list
        return polygons.Select(MapToDto).ToList();
    }

    // ========================================================================
    // GetByIdAsync - Get one polygon and convert to DTO
    // ========================================================================
    // Uses a ternary operator for concise null handling:
    //   polygon == null ? null : MapToDto(polygon)
    //
    // This means:
    //   "If polygon is null, return null. Otherwise, return MapToDto(polygon)."
    //
    // The Controller will check for null and return 404 if not found.
    // ========================================================================
    public async Task<PolygonDto?> GetByIdAsync(string id)
    {
        // Get the Polygon model (or null) from database
        var polygon = await _repository.GetByIdAsync(id);

        // If null, return null. Otherwise, convert to DTO and return.
        return polygon == null ? null : MapToDto(polygon);
    }

    // ========================================================================
    // CreateAsync - Validate, transform, and create a new polygon
    // ========================================================================
    // This is the most complex method because it does the most work:
    // 1. Validate the input
    // 2. Ensure the polygon is closed
    // 3. Convert coordinates to GeoJSON format
    // 4. Create the Model and save it
    // 5. Convert the result back to DTO
    //
    // THROWS: ArgumentException if validation fails
    // ========================================================================
    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request)
    {
        // ====================================================================
        // STEP 1: Validate the request
        // ====================================================================
        // Throws ArgumentException if:
        // - Less than 3 coordinates
        // - Any coordinate has invalid latitude/longitude
        // ====================================================================
        ValidatePolygon(request.Coordinates);

        // ====================================================================
        // STEP 2: Ensure the polygon is closed
        // ====================================================================
        // GeoJSON requires the first and last coordinate to be the same.
        // If the client forgot to close it, we do it for them.
        // This is defensive programming - we don't trust the client.
        // ====================================================================
        var coordinates = EnsureClosedPolygon(request.Coordinates);

        // ====================================================================
        // STEP 3: Create the Polygon model with GeoJSON geometry
        // ====================================================================
        // This is "object initializer" syntax in C#:
        //   new Polygon { Property = value }
        // It creates a new Polygon and sets properties in one step.
        //
        // CreateGeoJsonPolygon converts our simple coordinates to MongoDB's
        // complex GeoJsonPolygon type.
        // ====================================================================
        var polygon = new Polygon
        {
            Geometry = CreateGeoJsonPolygon(coordinates)
        };

        // ====================================================================
        // STEP 4: Save to database and get the result with ID
        // ====================================================================
        var created = await _repository.CreateAsync(polygon);

        // ====================================================================
        // STEP 5: Convert back to DTO and return
        // ====================================================================
        // The DTO is what the Controller will send to the client as JSON.
        return MapToDto(created);
    }

    // ========================================================================
    // DeleteAsync - Just delegate to repository
    // ========================================================================
    // No business logic needed - just pass through to repository.
    // Returns true if deleted, false if not found.
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
    // These are internal implementation details. They're "static" because they
    // don't need access to instance fields (_repository).
    //
    // "private" = only this class can use them
    // "static" = they don't need "this" - they're pure functions
    // ============================================================================

    // ========================================================================
    // ValidatePolygon - Check if the polygon data is valid
    // ========================================================================
    // THROWS: ArgumentException if invalid
    //
    // Rules:
    // 1. Must have at least 3 coordinates (triangle is minimum polygon)
    // 2. Each coordinate must be valid (see ValidateCoordinate)
    // ========================================================================
    private static void ValidatePolygon(List<Coordinate> coordinates)
    {
        // Rule 1: At least 3 coordinates
        if (coordinates.Count < 3)
        {
            // "throw" = stop execution and signal an error
            // "new ArgumentException(...)" = create an error object with a message
            // The Controller will catch this and return 400 Bad Request
            throw new ArgumentException("Polygon must have at least 3 coordinates");
        }

        // Rule 2: Each coordinate must be valid
        // "foreach" = iterate through each item in the list
        foreach (var coord in coordinates)
        {
            ValidateCoordinate(coord);  // Throws if invalid
        }
    }

    // ========================================================================
    // ValidateCoordinate - Check if a single coordinate is valid
    // ========================================================================
    // THROWS: ArgumentException if invalid
    //
    // Rules:
    // - Latitude: -90 to 90 (South Pole to North Pole)
    // - Longitude: -180 to 180 (International Date Line wraps)
    //
    // WHY VALIDATE?
    // Invalid coordinates would cause MongoDB errors or incorrect behavior.
    // Better to fail fast with a clear error message.
    // ========================================================================
    private static void ValidateCoordinate(Coordinate coord)
    {
        // Check latitude range
        if (coord.Latitude < -90 || coord.Latitude > 90)
        {
            // "$" before the string enables string interpolation
            // {coord.Latitude} is replaced with the actual value
            throw new ArgumentException($"Latitude must be between -90 and 90, got {coord.Latitude}");
        }

        // Check longitude range
        if (coord.Longitude < -180 || coord.Longitude > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {coord.Longitude}");
        }
    }

    // ========================================================================
    // EnsureClosedPolygon - Make sure first and last coordinates match
    // ========================================================================
    // GeoJSON spec requires polygons to be "closed" - the first and last
    // point must be identical. This method adds the closing point if missing.
    //
    // Example:
    //   Input:  [A, B, C]           (open - 3 points)
    //   Output: [A, B, C, A]        (closed - 4 points, last = first)
    //
    // If already closed, returns the original list unchanged.
    // ========================================================================
    private static List<Coordinate> EnsureClosedPolygon(List<Coordinate> coordinates)
    {
        // Get first and last coordinates
        var first = coordinates[0];        // First element
        var last = coordinates[^1];        // [^1] = last element (C# 8 "index from end" syntax)

        // Check if already closed (first equals last)
        if (first.Latitude == last.Latitude && first.Longitude == last.Longitude)
        {
            // Already closed, return as-is
            return coordinates;
        }

        // Not closed - create a new list with the first coordinate added at the end
        // "new List<Coordinate>(coordinates)" = copy the original list
        // "{ first }" = collection initializer - adds 'first' to the list
        var closed = new List<Coordinate>(coordinates) { first };
        return closed;
    }

    // ========================================================================
    // CreateGeoJsonPolygon - Convert simple coordinates to MongoDB GeoJSON
    // ========================================================================
    // This is the key transformation: simple List<Coordinate> → complex GeoJsonPolygon
    //
    // GeoJSON STRUCTURE (nested, complex):
    //   GeoJsonPolygon
    //     └── GeoJsonPolygonCoordinates
    //           └── GeoJsonLinearRingCoordinates (the outer ring)
    //                 └── List<GeoJson2DGeographicCoordinates> (the points)
    //
    // Our simple structure:
    //   List<Coordinate> (each with Latitude and Longitude)
    //
    // COORDINATE ORDER:
    // Our Coordinate: (Latitude, Longitude) - like "32.5, 35.2"
    // GeoJSON standard: (Longitude, Latitude) - like "[35.2, 32.5]"
    // We swap the order during conversion!
    // ========================================================================
    private static GeoJsonPolygon<GeoJson2DGeographicCoordinates> CreateGeoJsonPolygon(List<Coordinate> coordinates)
    {
        // Step 1: Convert each Coordinate to GeoJson2DGeographicCoordinates
        // Note: GeoJSON uses (Longitude, Latitude) order!
        var geoJsonCoordinates = coordinates.Select(c =>
            new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude)  // Longitude first!
        ).ToList();

        // Step 2: Create a linear ring (a closed loop of coordinates)
        // A polygon's outer boundary is called a "linear ring"
        var linearRing = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(geoJsonCoordinates);

        // Step 3: Create polygon coordinates (could have holes, but we don't use them)
        // Step 4: Create and return the final GeoJsonPolygon
        return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRing)
        );
    }

    // ========================================================================
    // MapToDto - Convert a Polygon model to a PolygonDto
    // ========================================================================
    // This is the reverse transformation: complex GeoJsonPolygon → simple DTO
    //
    // We extract the coordinates from the deeply nested GeoJSON structure
    // and convert them to our simple Coordinate objects.
    //
    // PROPERTY CHAIN EXPLAINED:
    //   polygon.Geometry              = the GeoJsonPolygon
    //          .Coordinates           = the GeoJsonPolygonCoordinates
    //          .Exterior              = the outer ring (GeoJsonLinearRingCoordinates)
    //          .Positions             = the list of GeoJson2DGeographicCoordinates
    // ========================================================================
    private static PolygonDto MapToDto(Polygon polygon)
    {
        // Extract coordinates from the nested GeoJSON structure
        var coordinates = polygon.Geometry.Coordinates.Exterior.Positions
            .Select(pos => new Coordinate
            {
                // GeoJSON stores (Longitude, Latitude) but our DTO uses (Latitude, Longitude)
                // So we swap back to our format
                Latitude = pos.Latitude,
                Longitude = pos.Longitude
            })
            .ToList();

        // Create and return the DTO
        return new PolygonDto
        {
            Id = polygon.Id,              // The MongoDB ObjectId as a string
            Coordinates = coordinates      // The simplified coordinate list
        };
    }
}
