using System.ComponentModel.DataAnnotations;

namespace MapServer.Application.DTOs;

/// <summary>
/// Request DTO for creating a map object (POST /api/objects).
/// </summary>
public record CreateMapObjectRequest
{
    /// <summary>
    /// Where to place this object on the map.
    /// </summary>
    [Required(ErrorMessage = "Location is required")]
    public Coordinate? Location { get; init; }

    /// <summary>
    /// What kind of object this is (e.g., "Marker", "Jeep", "Tank").
    /// </summary>
    [Required(ErrorMessage = "ObjectType is required")]
    public string? ObjectType { get; init; }
}
