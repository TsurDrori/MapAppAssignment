using MapServer.Domain.ValueObjects;

namespace MapServer.Domain.Entities;

/// <summary>
/// Domain entity representing a polygon on a map.
/// Contains a list of coordinates forming the polygon boundary.
/// </summary>
public class Polygon
{
    public string? Id { get; set; }

    /// <summary>
    /// The polygon boundary as a list of coordinates.
    /// Must have at least 3 coordinates and should be closed (first == last).
    /// </summary>
    public List<GeoCoordinate> Coordinates { get; set; } = [];
}
