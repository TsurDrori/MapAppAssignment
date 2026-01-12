using MapServer.Application.DTOs;
using MapServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MapServer.Api.Controllers;

/// <summary>
/// HTTP endpoint handler for map object operations.
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
        if (obj == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Object not found",
                Status = 404,
                Detail = $"Object with ID '{id}' was not found"
            });
        }

        return Ok(obj);
    }

    /// <summary>
    /// Create a single map object.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MapObjectDto>> Create(CreateMapObjectRequest request)
    {
        try
        {
            var created = await _mapObjectService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Create multiple map objects in one batch operation.
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<List<MapObjectDto>>> CreateBatch(BatchCreateMapObjectsRequest request)
    {
        try
        {
            var created = await _mapObjectService.CreateManyAsync(request);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete one map object by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mapObjectService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Object not found",
                Status = 404,
                Detail = $"Object with ID '{id}' was not found"
            });
        }

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
