using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MapServer.Api.Controllers;

/// <summary>
/// HTTP endpoint handler for map object operations.
/// Thin controller - all business logic in services, all error handling in middleware.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ObjectsController : ControllerBase
{
    private readonly IMapObjectService _mapObjectService;

    public ObjectsController(IMapObjectService mapObjectService)
    {
        _mapObjectService = mapObjectService;
    }

    /// <summary>
    /// Get all map objects.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MapObjectDto>>> GetAll()
    {
        var objects = await _mapObjectService.GetAllAsync();
        return Ok(objects);
    }

    /// <summary>
    /// Get one map object by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MapObjectDto>> GetById(string id)
    {
        var obj = await _mapObjectService.GetByIdAsync(id);
        return Ok(obj);
    }

    /// <summary>
    /// Create a single map object.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MapObjectDto>> Create(CreateMapObjectRequest request)
    {
        var created = await _mapObjectService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Create multiple map objects in one batch operation.
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<List<MapObjectDto>>> CreateBatch(BatchCreateMapObjectsRequest request)
    {
        var created = await _mapObjectService.CreateManyAsync(request);
        return Ok(created);
    }

    /// <summary>
    /// Delete one map object by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mapObjectService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Delete all map objects.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        await _mapObjectService.DeleteAllAsync();
        return NoContent();
    }
}
