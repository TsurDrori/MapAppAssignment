namespace MapServer.DTOs;

/// <summary>
/// Response DTO for map objects. Sent to client on GET /api/objects.
/// A map object is a point on the map with a type label (e.g., "Marker", "Jeep").
/// </summary>
public record MapObjectDto
{
    /// <summary>
    /// MongoDB ObjectId as string. Nullable because it doesn't exist until saved.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Where this object is on the map.
    /// </summary>
    public Coordinate Location { get; init; } = new();

    /// <summary>
    /// What kind of object this is (e.g., "Marker", "Jeep", "Tank").
    /// The server accepts any string - the client determines valid types.
    /// </summary>
    public string ObjectType { get; init; } = string.Empty;
}
