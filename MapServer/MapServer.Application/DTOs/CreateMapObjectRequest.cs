namespace MapServer.Application.DTOs;

/// <summary>
/// Request DTO for creating a map object (POST /api/objects).
/// </summary>
public record CreateMapObjectRequest
{
    /// <summary>
    /// Where to place this object on the map.
    /// </summary>
    public Coordinate Location { get; init; } = new();

    /// <summary>
    /// What kind of object this is (e.g., "Marker", "Jeep", "Tank").
    /// </summary>
    public string ObjectType { get; init; } = string.Empty;
}
