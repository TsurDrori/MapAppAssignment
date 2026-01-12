namespace MapServer.DTOs;

/// <summary>
/// Request DTO for creating a map object (POST /api/objects).
/// </summary>
public record CreateMapObjectRequest
{
    /// <summary>
    /// Where to place this object on the map.
    /// Coordinates are validated (lat: -90 to 90, lon: -180 to 180).
    /// </summary>
    public Coordinate Location { get; init; } = new();

    /// <summary>
    /// What kind of object this is (e.g., "Marker", "Jeep", "Tank").
    /// Not validated - any string is accepted.
    /// </summary>
    public string ObjectType { get; init; } = string.Empty;
}
