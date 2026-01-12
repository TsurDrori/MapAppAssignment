namespace MapServer.Application.DTOs;

/// <summary>
/// Request DTO for batch creating map objects (POST /api/objects/batch).
/// </summary>
public record BatchCreateMapObjectsRequest
{
    /// <summary>
    /// The objects to create.
    /// </summary>
    public List<CreateMapObjectRequest> Objects { get; init; } = [];
}
