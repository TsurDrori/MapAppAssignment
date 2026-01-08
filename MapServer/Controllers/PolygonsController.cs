// ============================================================================
// PolygonsController.cs - THE HTTP ENDPOINT HANDLER FOR POLYGONS
// ============================================================================
//
// WHAT: This is the "Controller" - the entry point for HTTP requests.
//       It defines the API endpoints (URLs) and handles HTTP concerns.
//
// WHY:  Controllers are the "thin" layer between HTTP and business logic:
//       - Receive HTTP requests
//       - Extract data from requests (URL params, body)
//       - Call the appropriate Service method
//       - Return HTTP responses with correct status codes
//
// CONTROLLERS ARE THIN:
//       No business logic here! No validation! No data transformation!
//       All of that is in the Service layer. The Controller just:
//       - Routes requests to services
//       - Handles HTTP-specific concerns (status codes, response format)
//
// ENDPOINTS DEFINED:
//       GET    /api/polygons      → GetAll()     → Get all polygons
//       GET    /api/polygons/{id} → GetById(id)  → Get one polygon
//       POST   /api/polygons      → Create()     → Create a polygon
//       DELETE /api/polygons/{id} → Delete(id)   → Delete one polygon
//       DELETE /api/polygons      → DeleteAll()  → Delete all polygons
//
// See BACKEND_CONCEPTS.md: Controllers, HTTP Methods, HTTP Status Codes
// ============================================================================

using MapServer.DTOs;       // For PolygonDto, CreatePolygonRequest
using MapServer.Services;   // For IPolygonService
using Microsoft.AspNetCore.Mvc;  // For ControllerBase, attributes, ActionResult, etc.

namespace MapServer.Controllers;

// ============================================================================
// CONTROLLER ATTRIBUTES - Configuration via "sticky notes"
// ============================================================================
//
// [ApiController]
// This attribute marks the class as an API controller, enabling:
// - Automatic model validation (if invalid, returns 400 without calling the method)
// - Automatic JSON response formatting
// - Automatic binding of request body to parameters
//
// [Route("api/[controller]")]
// This sets the base URL for all endpoints in this controller.
// "[controller]" is a placeholder that becomes the class name minus "Controller":
//   PolygonsController → "Polygons" → /api/polygons
//
// See BACKEND_CONCEPTS.md: Attributes (Square Brackets)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase   // Inherits from ControllerBase for helper methods
{
    // ========================================================================
    // PRIVATE FIELD - The service we'll use for business logic
    // ========================================================================
    // We depend on IPolygonService (interface), not PolygonService (concrete).
    // This is dependency injection - the actual service is provided by ASP.NET Core.
    // ========================================================================
    private readonly IPolygonService _polygonService;

    // ========================================================================
    // CONSTRUCTOR - Receives the service via dependency injection
    // ========================================================================
    // ASP.NET Core automatically:
    // 1. Creates a PolygonService (based on Program.cs registration)
    // 2. Passes it to this constructor
    // 3. Does this for each HTTP request (because service is "scoped")
    //
    // See BACKEND_CONCEPTS.md: Dependency Injection
    // ========================================================================
    public PolygonsController(IPolygonService polygonService)
    {
        _polygonService = polygonService;
    }

    // ========================================================================
    // GET /api/polygons - Get all polygons
    // ========================================================================
    //
    // [HttpGet]
    // This attribute says "this method handles GET requests to the base route".
    // The base route is /api/polygons (from [Route] on the class).
    //
    // RETURN TYPE: Task<ActionResult<List<PolygonDto>>>
    //   - Task = async method
    //   - ActionResult<T> = can return T or an HTTP error response
    //   - List<PolygonDto> = list of polygon DTOs
    //
    // EXAMPLE REQUEST:
    //   GET http://localhost:5102/api/polygons
    //
    // EXAMPLE RESPONSE (200 OK):
    //   [
    //     { "id": "507f...", "coordinates": [...] },
    //     { "id": "508f...", "coordinates": [...] }
    //   ]
    // ========================================================================
    [HttpGet]
    public async Task<ActionResult<List<PolygonDto>>> GetAll()
    {
        // Call service to get all polygons (already converted to DTOs)
        var polygons = await _polygonService.GetAllAsync();

        // Ok() is a helper method from ControllerBase
        // It returns HTTP 200 OK with the data as JSON
        return Ok(polygons);
    }

    // ========================================================================
    // GET /api/polygons/{id} - Get one polygon by ID
    // ========================================================================
    //
    // [HttpGet("{id}")]
    // The "{id}" means "there's a URL segment here called 'id'".
    // This segment is automatically bound to the 'id' parameter.
    //
    // EXAMPLE:
    //   GET /api/polygons/507f1f77bcf86cd799439011
    //   → id = "507f1f77bcf86cd799439011"
    //
    // POSSIBLE RESPONSES:
    //   200 OK - Polygon found, returns the polygon
    //   404 Not Found - No polygon with that ID
    // ========================================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<PolygonDto>> GetById(string id)
    {
        // Call service to get the polygon (returns null if not found)
        var polygon = await _polygonService.GetByIdAsync(id);

        // If null, return 404 Not Found with ProblemDetails
        if (polygon == null)
        {
            // NotFound() returns HTTP 404
            // ProblemDetails is a standard format for error responses (RFC 7807)
            return NotFound(new ProblemDetails
            {
                Title = "Polygon not found",       // Short description
                Status = 404,                      // HTTP status code
                Detail = $"Polygon with ID '{id}' was not found"  // Detailed message
            });
        }

        // Found! Return 200 OK with the polygon
        return Ok(polygon);
    }

    // ========================================================================
    // POST /api/polygons - Create a new polygon
    // ========================================================================
    //
    // [HttpPost]
    // This method handles POST requests (for creating new resources).
    //
    // PARAMETER: CreatePolygonRequest request
    // ASP.NET Core automatically:
    // 1. Reads the request body (JSON)
    // 2. Deserializes it to CreatePolygonRequest
    // 3. Passes it to this method
    //
    // This is called "model binding" - binding JSON to C# objects.
    //
    // EXAMPLE REQUEST:
    //   POST /api/polygons
    //   Content-Type: application/json
    //   { "coordinates": [{"latitude": 32.5, "longitude": 35.2}, ...] }
    //
    // POSSIBLE RESPONSES:
    //   201 Created - Success, includes Location header
    //   400 Bad Request - Validation failed (< 3 coords, invalid coords)
    // ========================================================================
    [HttpPost]
    public async Task<ActionResult<PolygonDto>> Create(CreatePolygonRequest request)
    {
        // try/catch to handle validation errors from the service
        try
        {
            // Call service to create the polygon
            // Service handles validation and throws ArgumentException if invalid
            var created = await _polygonService.CreateAsync(request);

            // CreatedAtAction returns HTTP 201 Created with:
            // - Location header: /api/polygons/{id} (URL of the new resource)
            // - Body: the created polygon DTO
            //
            // nameof(GetById) = "GetById" (string) - used to generate the Location URL
            // new { id = created.Id } = route values for GetById
            // created = the response body
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            // Validation failed (e.g., "Polygon must have at least 3 coordinates")
            // Return HTTP 400 Bad Request with error details
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message  // The error message from the exception
            });
        }
    }

    // ========================================================================
    // DELETE /api/polygons/{id} - Delete one polygon by ID
    // ========================================================================
    //
    // [HttpDelete("{id}")]
    // Handles DELETE requests with an ID in the URL.
    //
    // RETURN TYPE: IActionResult (not ActionResult<T>)
    // IActionResult = "some kind of HTTP response, but no specific data type"
    // Used when success means "no content" (nothing to return).
    //
    // POSSIBLE RESPONSES:
    //   204 No Content - Successfully deleted
    //   404 Not Found - No polygon with that ID
    // ========================================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        // Call service to delete (returns true if deleted, false if not found)
        var deleted = await _polygonService.DeleteAsync(id);

        // If not deleted (didn't exist), return 404
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Polygon not found",
                Status = 404,
                Detail = $"Polygon with ID '{id}' was not found"
            });
        }

        // Successfully deleted - return 204 No Content
        // NoContent() means "success, but nothing to return"
        // This is the standard response for successful DELETE
        return NoContent();
    }

    // ========================================================================
    // DELETE /api/polygons - Delete ALL polygons
    // ========================================================================
    //
    // [HttpDelete] (no path parameter)
    // Handles DELETE requests to the base route /api/polygons.
    //
    // CAUTION: This deletes EVERYTHING! No confirmation.
    // In a real app, you might want authentication or confirmation.
    //
    // RESPONSE:
    //   204 No Content - Always succeeds (even if there were 0 polygons)
    // ========================================================================
    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        // Call service to delete all polygons
        await _polygonService.DeleteAllAsync();

        // Return 204 No Content (success, nothing to return)
        return NoContent();
    }
}
