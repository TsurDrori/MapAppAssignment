using System.ComponentModel.DataAnnotations;

namespace MapServer.Application.DTOs;

/// <summary>
/// Request DTO for creating a polygon (POST /api/polygons).
/// </summary>
public record CreatePolygonRequest
{
    /// <summary>
    /// Points forming the polygon. Requires at least 3 coordinates.
    /// The polygon will be auto-closed if needed (first point added at end).
    /// </summary>
    [Required(ErrorMessage = "Coordinates are required")]
    [MinLength(3, ErrorMessage = "Polygon must have at least 3 coordinates")]
    public List<Coordinate>? Coordinates { get; init; }
}
