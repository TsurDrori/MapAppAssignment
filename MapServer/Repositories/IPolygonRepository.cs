// ============================================================================
// IPolygonRepository.cs - THE CONTRACT FOR POLYGON DATA ACCESS
// ============================================================================
//
// WHAT: An INTERFACE that defines WHAT operations can be done with polygons.
//       It's a contract - it says "any polygon repository must be able to do these things".
//
// WHY:  Interfaces let us:
//       1. Swap implementations (real database vs. mock for testing)
//       2. Define a contract without implementation details
//       3. Enable dependency injection (Program.cs registers the interface)
//
// WHO USES THIS:
//       - PolygonService depends on IPolygonRepository (not the concrete class)
//       - Program.cs registers: IPolygonRepository → PolygonRepository
//       - In tests, we could register: IPolygonRepository → MockPolygonRepository
//
// See BACKEND_CONCEPTS.md: Interfaces, Repositories, Dependency Injection
// ============================================================================

using MapServer.Models;  // For the Polygon type

namespace MapServer.Repositories;

// ============================================================================
// IPolygonRepository INTERFACE
// ============================================================================
// "public interface" = defines a contract that classes must implement
// "I" prefix = C# convention for interfaces
//
// This interface defines 5 operations:
// - GetAllAsync: Get all polygons
// - GetByIdAsync: Get one polygon by ID
// - CreateAsync: Create a new polygon
// - DeleteAsync: Delete one polygon by ID
// - DeleteAllAsync: Delete all polygons
//
// NOTE: No UpdateAsync! The assignment says Create/Delete only.
// To "edit", the client deletes the old polygon and creates a new one.
// ============================================================================
public interface IPolygonRepository
{
    // ========================================================================
    // GetAllAsync - Retrieve all polygons from the database
    // ========================================================================
    // Returns: Task<List<Polygon>>
    //   - Task = this is async (returns a promise)
    //   - List<Polygon> = a list of Polygon objects
    //
    // WHY RETURN A LIST?
    // The client needs all polygons to display them on the map.
    // For small datasets, returning all is fine. For large datasets,
    // you'd add pagination (but that's not in this project's scope).
    //
    // See BACKEND_CONCEPTS.md: Async/Await, Generic Types
    // ========================================================================
    Task<List<Polygon>> GetAllAsync();

    // ========================================================================
    // GetByIdAsync - Retrieve one polygon by its ID
    // ========================================================================
    // Parameters:
    //   - id: The MongoDB ObjectId as a string (e.g., "507f1f77bcf86cd799439011")
    //
    // Returns: Task<Polygon?>
    //   - Polygon? = A polygon OR null (if not found)
    //   - The "?" means nullable - the polygon might not exist
    //
    // WHY NULLABLE RETURN?
    // If someone asks for a polygon that doesn't exist (wrong ID),
    // we return null instead of throwing an exception. The Service
    // layer can then decide how to handle it (return 404 Not Found).
    //
    // See BACKEND_CONCEPTS.md: Null Safety
    // ========================================================================
    Task<Polygon?> GetByIdAsync(string id);

    // ========================================================================
    // CreateAsync - Save a new polygon to the database
    // ========================================================================
    // Parameters:
    //   - polygon: The Polygon object to save (with Geometry, but Id is null)
    //
    // Returns: Task<Polygon>
    //   - The same polygon, but now with an Id assigned by MongoDB
    //
    // THE FLOW:
    //   1. Service creates a Polygon object with Geometry but no Id
    //   2. Repository calls InsertOneAsync (MongoDB driver method)
    //   3. MongoDB generates an _id and stores the document
    //   4. MongoDB driver updates the polygon object's Id property
    //   5. Repository returns the polygon with its new Id
    //
    // WHY RETURN THE POLYGON?
    // The caller needs the generated ID to return to the client.
    // The 201 Created response includes the new resource's location.
    // ========================================================================
    Task<Polygon> CreateAsync(Polygon polygon);

    // ========================================================================
    // DeleteAsync - Delete one polygon by its ID
    // ========================================================================
    // Parameters:
    //   - id: The MongoDB ObjectId as a string
    //
    // Returns: Task<bool>
    //   - true = polygon was found and deleted
    //   - false = no polygon with that ID exists
    //
    // WHY RETURN BOOL?
    // The caller needs to know if anything was actually deleted.
    // If false, the Service can tell the Controller to return 404.
    // ========================================================================
    Task<bool> DeleteAsync(string id);

    // ========================================================================
    // DeleteAllAsync - Delete ALL polygons from the database
    // ========================================================================
    // Returns: Task (no value, just awaitable)
    //
    // WHY NO RETURN VALUE?
    // We don't need to know how many were deleted. This operation
    // always "succeeds" (even if there were 0 polygons to delete).
    //
    // USE CASE:
    // The client can call DELETE /api/polygons to clear all polygons,
    // useful for resetting the map or starting fresh.
    //
    // NOTE: This is a "void" returning Task. In C#:
    //   - Task<int> = async method that returns an int
    //   - Task = async method that returns nothing (void)
    // ========================================================================
    Task DeleteAllAsync();
}
