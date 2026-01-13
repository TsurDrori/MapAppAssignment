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
    public async Task<ActionResult<List<PolygonDto>>> GetAll()
    {
        var polygons = await _polygonService.GetAllAsync();//CHECK
        return Ok(polygons);
    }

    /// <summary>
    /// Get one polygon by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PolygonDto>> GetById(string id)
    {
        var polygon = await _polygonService.GetByIdAsync(id);
        return Ok(polygon);
    }

    /// <summary>
    /// Create a new polygon.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PolygonDto>> Create(CreatePolygonRequest request)
    {
        var created = await _polygonService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Delete one polygon by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _polygonService.DeleteAsync(id);
        return NoContent();
    }
}
