using System.ComponentModel.DataAnnotations;

namespace MapServer.Application.DTOs;

/// <summary>
/// Request DTO for batch creating map objects (POST /api/objects/batch).
/// </summary>
public record BatchCreateMapObjectsRequest
{
    /// <summary>
    /// The objects to create.
    /// </summary>
    [Required(ErrorMessage = "Objects are required")]
    [MinLength(1, ErrorMessage = "At least one object is required")]
    public List<CreateMapObjectRequest> Objects { get; init; } = [];
}
