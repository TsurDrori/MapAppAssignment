using MapServer.Domain.ValueObjects;

namespace MapServer.Domain.Entities;

/// <summary>
/// Domain entity representing a map object (marker, vehicle, etc.).
/// </summary>
public class MapObject
{
    public string? Id { get; set; }

    /// <summary>
    /// The location of this object on the map.
    /// </summary>
    public GeoCoordinate Location { get; set; } = null!;

    /// <summary>
    /// The type of object (e.g., "Marker", "Jeep", "Tank").
    /// No server-side validation - client determines valid types.
    /// </summary>
    public string ObjectType { get; set; } = string.Empty;
}
