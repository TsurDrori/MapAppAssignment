namespace MapServer.DTOs;

/// <summary>
/// Request DTO for creating a polygon (POST /api/polygons).
/// No ID field - MongoDB generates it on save.
/// </summary>
public record CreatePolygonRequest
{
    /// <summary>
    /// Points forming the polygon. Requires at least 3 coordinates.
    /// The polygon will be auto-closed if needed (first point added at end).
    /// </summary>
    public List<Coordinate> Coordinates { get; init; } = [];
}
