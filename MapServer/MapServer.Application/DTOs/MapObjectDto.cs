namespace MapServer.Application.DTOs;

/// <summary>
/// Response DTO for map objects. Sent to client on GET /api/objects.
/// </summary>
public record MapObjectDto
{
    /// <summary>
    /// MongoDB ObjectId as string.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Where this object is on the map.
    /// </summary>
    public Coordinate Location { get; init; } = new();

    /// <summary>
    /// What kind of object this is (e.g., "Marker", "Jeep", "Tank").
    /// </summary>
    public string ObjectType { get; init; } = string.Empty;
}
