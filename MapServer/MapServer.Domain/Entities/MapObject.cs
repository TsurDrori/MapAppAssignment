using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Domain.Entities;

/// <summary>
/// Domain entity representing a map object (marker, vehicle, etc.).
/// Uses GeoJSON (RFC 7946) for geographic representation.
/// GeoJSON is an open standard - this is domain knowledge, not persistence concern.
/// </summary>
public class MapObject
{
    public string? Id { get; set; }

    /// <summary>
    /// The location as a GeoJSON Point.
    /// GeoJSON is an open standard, not a MongoDB-specific concern.
    /// </summary>
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = null!;

    /// <summary>
    /// The type of object (e.g., "Marker", "Jeep", "Tank").
    /// No server-side validation - client determines valid types.
    /// </summary>
    public string ObjectType { get; set; } = string.Empty;
}
