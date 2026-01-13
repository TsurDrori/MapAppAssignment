using System.ComponentModel.DataAnnotations;

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
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; init; }

    /// <summary>
    /// East/West position. Range: -180 to +180. Prime Meridian (London) is 0.
    /// </summary>
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; init; }
}
