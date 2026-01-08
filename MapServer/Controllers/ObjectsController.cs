// ============================================================================
// ObjectsController.cs - THE HTTP ENDPOINT HANDLER FOR MAP OBJECTS
// ============================================================================
//
// WHAT: Controller for map objects (markers, vehicles, etc.).
//       Similar to PolygonsController, but adds batch creation endpoint.
//
// WHY:  Same pattern as PolygonsController - thin layer that:
//       - Routes HTTP requests to the service
//       - Handles status codes and error responses
//       - Does NOT contain business logic
//
// ENDPOINTS DEFINED:
//       GET    /api/objects        → GetAll()       → Get all objects
//       GET    /api/objects/{id}   → GetById(id)    → Get one object
//       POST   /api/objects        → Create()       → Create one object
//       POST   /api/objects/batch  → CreateBatch()  → Create multiple objects
//       DELETE /api/objects/{id}   → Delete(id)     → Delete one object
//       DELETE /api/objects        → DeleteAll()    → Delete all objects
//
// See BACKEND_CONCEPTS.md: Controllers, HTTP Methods, HTTP Status Codes
// ============================================================================

using MapServer.DTOs;
using MapServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace MapServer.Controllers;

// ============================================================================
// CONTROLLER CONFIGURATION
// ============================================================================
// [ApiController] - Enables API features (auto validation, JSON formatting)
// [Route("api/[controller]")] - Base URL is /api/objects
//   (ObjectsController → "Objects" → /api/objects)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class ObjectsController : ControllerBase
{
    // The service for business logic
    private readonly IMapObjectService _mapObjectService;

    // Constructor - receives service via dependency injection
    public ObjectsController(IMapObjectService mapObjectService)
    {
        _mapObjectService = mapObjectService;
    }

    // ========================================================================
    // GET /api/objects - Get all map objects
    // ========================================================================
    // Same pattern as PolygonsController.GetAll()
    // Returns all objects as JSON array.
    //
    // EXAMPLE RESPONSE:
    //   [
    //     { "id": "...", "location": {...}, "objectType": "Marker" },
    //     { "id": "...", "location": {...}, "objectType": "Jeep" }
    //   ]
    // ========================================================================
    [HttpGet]
    public async Task<ActionResult<List<MapObjectDto>>> GetAll()
    {
        var objects = await _mapObjectService.GetAllAsync();
        return Ok(objects);
    }

    // ========================================================================
    // GET /api/objects/{id} - Get one map object by ID
    // ========================================================================
    // Same pattern as PolygonsController.GetById()
    // Returns the object or 404 if not found.
    // ========================================================================
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

    // ========================================================================
    // POST /api/objects - Create a single map object
    // ========================================================================
    // Same pattern as PolygonsController.Create()
    // Creates one object from the request body.
    //
    // EXAMPLE REQUEST:
    //   POST /api/objects
    //   { "location": {"latitude": 32.5, "longitude": 35.2}, "objectType": "Marker" }
    //
    // RESPONSES:
    //   201 Created - Success
    //   400 Bad Request - Invalid coordinates
    // ========================================================================
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

    // ========================================================================
    // POST /api/objects/batch - Create multiple map objects at once
    // ========================================================================
    //
    // [HttpPost("batch")]
    // The "batch" is appended to the base route: /api/objects/batch
    //
    // WHY A SEPARATE ENDPOINT?
    // - Different URL makes the API clear and RESTful
    // - Different request body format (array of objects)
    // - Different response (returns all created objects)
    //
    // EXAMPLE REQUEST:
    //   POST /api/objects/batch
    //   {
    //     "objects": [
    //       { "location": {...}, "objectType": "Marker" },
    //       { "location": {...}, "objectType": "Jeep" },
    //       { "location": {...}, "objectType": "Tank" }
    //     ]
    //   }
    //
    // EXAMPLE RESPONSE (200 OK):
    //   [
    //     { "id": "...", "location": {...}, "objectType": "Marker" },
    //     { "id": "...", "location": {...}, "objectType": "Jeep" },
    //     { "id": "...", "location": {...}, "objectType": "Tank" }
    //   ]
    //
    // RESPONSES:
    //   200 OK - Success, returns all created objects with IDs
    //   400 Bad Request - Any object had invalid coordinates
    //
    // NOTE: Returns 200 OK (not 201 Created) because there's no single
    // "location" for the batch - we return the array directly.
    // ========================================================================
    [HttpPost("batch")]
    public async Task<ActionResult<List<MapObjectDto>>> CreateBatch(BatchCreateMapObjectsRequest request)
    {
        try
        {
            // Call service to create all objects
            var created = await _mapObjectService.CreateManyAsync(request);

            // Return 200 OK with the array of created objects
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            // Any validation failure fails the entire batch
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
    }

    // ========================================================================
    // DELETE /api/objects/{id} - Delete one map object by ID
    // ========================================================================
    // Same pattern as PolygonsController.Delete()
    //
    // RESPONSES:
    //   204 No Content - Successfully deleted
    //   404 Not Found - Object didn't exist
    // ========================================================================
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

    // ========================================================================
    // DELETE /api/objects - Delete ALL map objects
    // ========================================================================
    // Same pattern as PolygonsController.DeleteAll()
    // Deletes everything in the objects collection.
    //
    // RESPONSE:
    //   204 No Content - Always succeeds
    // ========================================================================
    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        await _mapObjectService.DeleteAllAsync();
        return NoContent();
    }
}
