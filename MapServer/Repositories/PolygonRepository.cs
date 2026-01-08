// ============================================================================
// PolygonRepository.cs - THE ACTUAL DATABASE OPERATIONS FOR POLYGONS
// ============================================================================
//
// WHAT: The concrete implementation of IPolygonRepository.
//       This class actually talks to MongoDB to store/retrieve polygons.
//
// WHY:  The Repository pattern separates "what we want to do" (interface)
//       from "how we do it" (this implementation). Benefits:
//       - Service layer doesn't know about MongoDB details
//       - Easy to test: swap this for a mock implementation
//       - Easy to change: switch databases without changing services
//
// HOW IT WORKS:
//       1. Receives MongoDbContext via dependency injection
//       2. Uses the Polygons collection from the context
//       3. Calls MongoDB driver methods (Find, InsertOne, DeleteOne, etc.)
//
// See BACKEND_CONCEPTS.md: Repositories, Dependency Injection, MongoDB Basics
// ============================================================================

using MapServer.Data;    // For MongoDbContext
using MapServer.Models;  // For Polygon
using MongoDB.Driver;    // For IMongoCollection, Find, etc.

namespace MapServer.Repositories;

// ============================================================================
// PolygonRepository CLASS
// ============================================================================
// ": IPolygonRepository" means "this class implements the IPolygonRepository interface"
// It MUST provide implementations for all methods defined in the interface.
//
// "public class" = accessible from anywhere, is a blueprint for creating objects
// ============================================================================
public class PolygonRepository : IPolygonRepository
{
    // ========================================================================
    // PRIVATE FIELD - The MongoDB collection we'll work with
    // ========================================================================
    // "private" = only this class can access it
    // "readonly" = can only be set in constructor, never changed after
    // "IMongoCollection<Polygon>" = a MongoDB collection that stores Polygons
    //
    // This is like a reference to the "polygons" table in SQL terms.
    // All our operations use this collection.
    // ========================================================================
    private readonly IMongoCollection<Polygon> _polygons;

    // ========================================================================
    // CONSTRUCTOR - Called when the object is created
    // ========================================================================
    // ASP.NET Core's dependency injection:
    // 1. Sees PolygonRepository needs a MongoDbContext
    // 2. Looks up MongoDbContext in the service container
    // 3. Passes it to this constructor
    //
    // We grab the Polygons collection and store it for later use.
    //
    // See BACKEND_CONCEPTS.md: Dependency Injection
    // ========================================================================
    public PolygonRepository(MongoDbContext context)
    {
        // Get the Polygons collection from the context and store it
        // Now we can use _polygons.Find(), _polygons.InsertOne(), etc.
        _polygons = context.Polygons;
    }

    // ========================================================================
    // GetAllAsync - Get all polygons from the database
    // ========================================================================
    // "public" = accessible from outside (required by interface)
    // "async" = this method uses async/await
    // "Task<List<Polygon>>" = returns a promise that resolves to a list of polygons
    //
    // MongoDB Driver Pattern:
    //   _polygons.Find(filter).ToListAsync()
    //
    // The filter "_ => true" means "match all documents".
    // - "_" is a throwaway parameter (we don't use it)
    // - "=> true" means "always return true" (match everything)
    //
    // It's like SQL: SELECT * FROM polygons (no WHERE clause)
    //
    // See BACKEND_CONCEPTS.md: Async/Await, Lambda Expressions
    // ========================================================================
    public async Task<List<Polygon>> GetAllAsync()
    {
        // Find all documents (filter matches everything)
        // ToListAsync() executes the query and returns results as a List
        return await _polygons.Find(_ => true).ToListAsync();
    }

    // ========================================================================
    // GetByIdAsync - Get one polygon by its ID
    // ========================================================================
    // "Polygon?" return type = might return null (polygon not found)
    //
    // MongoDB Driver Pattern:
    //   _polygons.Find(p => p.Id == id).FirstOrDefaultAsync()
    //
    // The filter "p => p.Id == id" means:
    //   "Given a polygon p, check if p's Id equals the provided id"
    //
    // FirstOrDefaultAsync():
    //   - Returns the first matching document
    //   - Returns null if no match (that's the "OrDefault" part)
    //
    // It's like SQL: SELECT * FROM polygons WHERE _id = 'xxx' LIMIT 1
    // ========================================================================
    public async Task<Polygon?> GetByIdAsync(string id)
    {
        // Find polygon where Id matches, return first match or null
        return await _polygons.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    // ========================================================================
    // CreateAsync - Insert a new polygon into the database
    // ========================================================================
    // Parameters:
    //   - polygon: The polygon to save (has Geometry, Id is null)
    //
    // Returns: The same polygon object, but now with Id filled in
    //
    // MongoDB Driver Pattern:
    //   _polygons.InsertOneAsync(document)
    //
    // MAGIC HAPPENS:
    // When InsertOneAsync completes, MongoDB:
    // 1. Generates a new ObjectId
    // 2. Stores the document with that _id
    // 3. The driver UPDATES the polygon.Id property automatically!
    //
    // So we can just return the same polygon object - it now has an Id.
    //
    // It's like SQL: INSERT INTO polygons VALUES (...) RETURNING *
    // ========================================================================
    public async Task<Polygon> CreateAsync(Polygon polygon)
    {
        // Insert the polygon document into MongoDB
        // This modifies polygon.Id to be the generated ObjectId
        await _polygons.InsertOneAsync(polygon);

        // Return the polygon (now has Id set by MongoDB)
        return polygon;
    }

    // ========================================================================
    // DeleteAsync - Delete one polygon by its ID
    // ========================================================================
    // Returns: true if a polygon was deleted, false if no match found
    //
    // MongoDB Driver Pattern:
    //   _polygons.DeleteOneAsync(filter)
    //
    // The result contains DeletedCount:
    //   - 1 = one document was deleted
    //   - 0 = no document matched the filter (didn't exist)
    //
    // It's like SQL: DELETE FROM polygons WHERE _id = 'xxx'
    // ========================================================================
    public async Task<bool> DeleteAsync(string id)
    {
        // Delete the polygon with matching Id
        // The filter "p => p.Id == id" finds the document to delete
        var result = await _polygons.DeleteOneAsync(p => p.Id == id);

        // Return true if at least one document was deleted
        // result.DeletedCount is a long (64-bit integer)
        return result.DeletedCount > 0;
    }

    // ========================================================================
    // DeleteAllAsync - Delete ALL polygons from the database
    // ========================================================================
    // Returns: Task (nothing - we don't need to know the count)
    //
    // MongoDB Driver Pattern:
    //   _polygons.DeleteManyAsync(filter)
    //
    // The filter "_ => true" matches all documents (same as GetAllAsync).
    // This deletes EVERYTHING in the collection.
    //
    // It's like SQL: DELETE FROM polygons (no WHERE clause)
    //
    // WARNING: This is destructive! There's no undo. In a real app,
    // you might want to add confirmation or soft-delete instead.
    // ========================================================================
    public async Task DeleteAllAsync()
    {
        // Delete all documents (filter matches everything)
        // We don't need the result, so we just await without capturing it
        await _polygons.DeleteManyAsync(_ => true);
    }
}
