namespace MapServer.DTOs;

/// <summary>
/// Response DTO for polygons. Sent to client on GET /api/polygons.
/// </summary>
public record PolygonDto
{
    /// <summary>
    /// MongoDB ObjectId as string. Nullable because it doesn't exist until saved.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Points forming the polygon. First and last coordinate should be the same (closed shape).
    /// </summary>
    public List<Coordinate> Coordinates { get; init; } = [];
}
