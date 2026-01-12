namespace MapServer.DTOs;

/// <summary>
/// Request DTO for batch creating map objects (POST /api/objects/batch).
/// More efficient than individual requests - one network call, one DB operation.
/// </summary>
public record BatchCreateMapObjectsRequest
{
    /// <summary>
    /// The objects to create. All validated individually; if any fails, entire batch fails.
    /// </summary>
    public List<CreateMapObjectRequest> Objects { get; init; } = [];
}
