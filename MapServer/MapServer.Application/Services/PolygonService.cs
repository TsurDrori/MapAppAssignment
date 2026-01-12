using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using MapServer.Domain.Entities;
using MapServer.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Application.Services;

/// <summary>
/// Business logic for polygon operations.
/// Handles validation, coordinate transformation, and DTO mapping.
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
        ValidatePolygon(request.Coordinates);

        var coordinates = EnsureClosedPolygon(request.Coordinates);
        ValidateLinearRingDoesNotSelfIntersect(coordinates);

        var polygon = new Polygon
        {
            Geometry = CreateGeoJsonPolygon(coordinates)
        };

        Polygon created;
        try
        {
            created = await _repository.CreateAsync(polygon);
        }
        catch (MongoWriteException ex) when (IsMongoInvalidPolygon(ex))
        {
            throw new ArgumentException("Polygon edges must not cross (self-intersection detected).");
        }

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

    private static void ValidatePolygon(List<Coordinate> coordinates)
    {
        if (coordinates.Count < 3)
        {
            throw new ArgumentException("Polygon must have at least 3 coordinates");
        }

        foreach (var coord in coordinates)
        {
            ValidateCoordinate(coord);
        }
    }

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

    private static List<Coordinate> EnsureClosedPolygon(List<Coordinate> coordinates)
    {
        var first = coordinates[0];
        var last = coordinates[^1];

        if (first.Latitude == last.Latitude && first.Longitude == last.Longitude)
        {
            return coordinates;
        }

        return [.. coordinates, first];
    }

    private static GeoJsonPolygon<GeoJson2DGeographicCoordinates> CreateGeoJsonPolygon(List<Coordinate> coordinates)
    {
        var geoJsonCoordinates = coordinates.Select(c =>
            new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude)
        ).ToList();

        var linearRing = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(geoJsonCoordinates);

        return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRing)
        );
    }

    private static void ValidateLinearRingDoesNotSelfIntersect(List<Coordinate> closedCoordinates)
    {
        if (closedCoordinates.Count < 4)
        {
            throw new ArgumentException("Polygon must have at least 3 distinct coordinates.");
        }

        var first = closedCoordinates[0];
        var last = closedCoordinates[^1];
        if (first.Latitude != last.Latitude || first.Longitude != last.Longitude)
        {
            throw new ArgumentException("Polygon must be closed (first coordinate must equal last).");
        }

        for (var i = 0; i < closedCoordinates.Count - 1; i++)
        {
            var a1 = ToPoint(closedCoordinates[i]);
            var a2 = ToPoint(closedCoordinates[i + 1]);

            if (PointsEqual(a1, a2))
            {
                throw new ArgumentException($"Polygon contains a zero-length edge at index {i}.");
            }

            for (var j = i + 1; j < closedCoordinates.Count - 1; j++)
            {
                if (AreAdjacentSegments(i, j, closedCoordinates.Count - 1))
                {
                    continue;
                }

                var b1 = ToPoint(closedCoordinates[j]);
                var b2 = ToPoint(closedCoordinates[j + 1]);

                if (SegmentsIntersect(a1, a2, b1, b2))
                {
                    throw new ArgumentException("Polygon edges must not cross (self-intersection detected).");
                }
            }
        }
    }

    private static bool IsMongoInvalidPolygon(MongoWriteException ex)
    {
        var message = ex.WriteError?.Message ?? string.Empty;
        return message.Contains("Loop is not valid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase)
            || ex.WriteError?.Code == 16755;
    }

    private readonly record struct Point(double X, double Y);

    private static Point ToPoint(Coordinate coordinate) => new(coordinate.Longitude, coordinate.Latitude);

    private static bool AreAdjacentSegments(int i, int j, int segmentCount)
    {
        if (j == i + 1)
        {
            return true;
        }

        // First and last segments share a vertex (closure).
        return i == 0 && j == segmentCount - 1;
    }

    private static bool SegmentsIntersect(Point p1, Point q1, Point p2, Point q2)
    {
        var o1 = Orientation(p1, q1, p2);
        var o2 = Orientation(p1, q1, q2);
        var o3 = Orientation(p2, q2, p1);
        var o4 = Orientation(p2, q2, q1);

        if (o1 != o2 && o3 != o4)
        {
            return true;
        }

        if (o1 == 0 && OnSegment(p1, p2, q1))
        {
            return true;
        }

        if (o2 == 0 && OnSegment(p1, q2, q1))
        {
            return true;
        }

        if (o3 == 0 && OnSegment(p2, p1, q2))
        {
            return true;
        }

        if (o4 == 0 && OnSegment(p2, q1, q2))
        {
            return true;
        }

        return false;
    }

    private static int Orientation(Point p, Point q, Point r)
    {
        const double epsilon = 1e-12;

        var cross = (q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X);

        if (Math.Abs(cross) <= epsilon)
        {
            return 0;
        }

        return cross > 0 ? 1 : -1;
    }

    private static bool OnSegment(Point p, Point q, Point r)
    {
        const double epsilon = 1e-12;

        return q.X <= Math.Max(p.X, r.X) + epsilon
            && q.X >= Math.Min(p.X, r.X) - epsilon
            && q.Y <= Math.Max(p.Y, r.Y) + epsilon
            && q.Y >= Math.Min(p.Y, r.Y) - epsilon;
    }

    private static bool PointsEqual(Point a, Point b)
    {
        const double epsilon = 1e-12;
        return Math.Abs(a.X - b.X) <= epsilon && Math.Abs(a.Y - b.Y) <= epsilon;
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
