using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Domain.Entities;

/// <summary>
/// Domain entity representing a polygon.
/// Uses GeoJSON (RFC 7946) for geographic representation.
/// GeoJSON is an open standard - this is domain knowledge, not persistence concern.
/// </summary>
public class Polygon
{
    public string? Id { get; set; }

    /// <summary>
    /// The polygon shape in GeoJSON format.
    /// GeoJSON is an open standard, not a MongoDB-specific concern.
    /// </summary>
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; } = null!;
}
