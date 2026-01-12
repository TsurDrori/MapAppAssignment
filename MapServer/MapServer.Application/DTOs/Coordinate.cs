namespace MapServer.Application.DTOs;

/// <summary>
/// A simple latitude/longitude point for API requests and responses.
/// Uses a simple format (not MongoDB's GeoJSON) to keep the API database-agnostic.
/// </summary>
public record Coordinate
{
    /// <summary>
    /// North/South position. Range: -90 (South Pole) to +90 (North Pole).
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// East/West position. Range: -180 to +180. Prime Meridian (London) is 0.
    /// </summary>
    public double Longitude { get; init; }
}
