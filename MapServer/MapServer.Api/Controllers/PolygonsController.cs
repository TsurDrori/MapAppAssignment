using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MapServer.Api.Controllers;

/// <summary>
/// HTTP endpoint handler for polygon operations.
/// Thin controller - all business logic in services, all error handling in middleware.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase
{
    private readonly IPolygonService _polygonService;

    public PolygonsController(IPolygonService polygonService)
    {
        _polygonService = polygonService;
    }

    /// <summary>
    /// Get all polygons.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PolygonDto>>> GetAll(CancellationToken cancellationToken)
    {
        var polygons = await _polygonService.GetAllAsync(cancellationToken);
        return Ok(polygons);
    }

    /// <summary>
    /// Get one polygon by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PolygonDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var polygon = await _polygonService.GetByIdAsync(id, cancellationToken);
        return Ok(polygon);
    }

    /// <summary>
    /// Create a new polygon.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PolygonDto>> Create(CreatePolygonRequest request, CancellationToken cancellationToken)
    {
        var created = await _polygonService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Delete one polygon by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _polygonService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
