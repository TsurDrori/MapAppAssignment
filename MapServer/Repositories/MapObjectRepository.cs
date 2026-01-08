// ============================================================================
// MapObjectRepository.cs - DATABASE OPERATIONS FOR MAP OBJECTS
// ============================================================================
//
// WHAT: The concrete implementation of IMapObjectRepository.
//       Handles all MongoDB operations for map objects.
//
// WHY:  Same pattern as PolygonRepository - separates database operations
//       from business logic. Services don't know about MongoDB.
//
// DIFFERENCE FROM PolygonRepository:
//       Has CreateManyAsync for efficient batch insertion.
//
// See BACKEND_CONCEPTS.md: Repositories, Dependency Injection, MongoDB Basics
// ============================================================================

using MapServer.Data;    // For MongoDbContext
using MapServer.Models;  // For MapObject
using MongoDB.Driver;    // For IMongoCollection, Find, etc.

namespace MapServer.Repositories;

// ============================================================================
// MapObjectRepository CLASS
// ============================================================================
// ": IMapObjectRepository" = implements the IMapObjectRepository interface
// Must provide all methods defined in the interface.
// ============================================================================
public class MapObjectRepository : IMapObjectRepository
{
    // ========================================================================
    // PRIVATE FIELD - The MongoDB collection for map objects
    // ========================================================================
    // Stores a reference to the "objects" collection in MongoDB.
    // All operations in this class use this collection.
    // ========================================================================
    private readonly IMongoCollection<MapObject> _mapObjects;

    // ========================================================================
    // CONSTRUCTOR - Receives MongoDbContext via dependency injection
    // ========================================================================
    // Gets the MapObjects collection from the context.
    // The context is a singleton, so this collection reference is always valid.
    // ========================================================================
    public MapObjectRepository(MongoDbContext context)
    {
        _mapObjects = context.MapObjects;
    }

    // ========================================================================
    // GetAllAsync - Get all map objects
    // ========================================================================
    // Same pattern as PolygonRepository:
    // - Find(_ => true) matches all documents
    // - ToListAsync() executes and returns as list
    // ========================================================================
    public async Task<List<MapObject>> GetAllAsync()
    {
        return await _mapObjects.Find(_ => true).ToListAsync();
    }

    // ========================================================================
    // GetByIdAsync - Get one map object by ID
    // ========================================================================
    // Same pattern as PolygonRepository:
    // - Find with Id filter
    // - FirstOrDefaultAsync returns match or null
    // ========================================================================
    public async Task<MapObject?> GetByIdAsync(string id)
    {
        return await _mapObjects.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    // ========================================================================
    // CreateAsync - Insert one map object
    // ========================================================================
    // Same pattern as PolygonRepository:
    // - InsertOneAsync saves the document
    // - MongoDB assigns an Id
    // - Return the object with its new Id
    // ========================================================================
    public async Task<MapObject> CreateAsync(MapObject mapObject)
    {
        await _mapObjects.InsertOneAsync(mapObject);
        return mapObject;
    }

    // ========================================================================
    // CreateManyAsync - Insert multiple map objects at once (BATCH)
    // ========================================================================
    // This is the key difference from PolygonRepository!
    //
    // MongoDB Driver Pattern:
    //   _mapObjects.InsertManyAsync(listOfDocuments)
    //
    // HOW IT WORKS:
    //   1. Receives a list of MapObject (each has Location/ObjectType, no Id)
    //   2. Calls InsertManyAsync to insert ALL of them in one operation
    //   3. MongoDB assigns Ids to ALL documents
    //   4. The driver updates each object's Id property
    //   5. Returns the list with Ids now filled in
    //
    // PERFORMANCE COMPARISON:
    //   Individual inserts:  100 objects = 100 network calls = ~500ms
    //   Batch insert:        100 objects = 1 network call   = ~10ms
    //
    // WHY SO MUCH FASTER?
    //   - One network round-trip instead of many
    //   - One write operation in the database
    //   - Less overhead per document
    //
    // ATOMICITY NOTE:
    //   InsertManyAsync is NOT atomic by default. If one document fails,
    //   earlier documents might already be inserted. For this app, that's OK.
    //   If you needed all-or-nothing, you'd use a transaction.
    // ========================================================================
    public async Task<List<MapObject>> CreateManyAsync(List<MapObject> mapObjects)
    {
        // Insert all documents in one database call
        // This is MUCH faster than calling InsertOneAsync in a loop
        await _mapObjects.InsertManyAsync(mapObjects);

        // Return the list - all objects now have Ids assigned
        return mapObjects;
    }

    // ========================================================================
    // DeleteAsync - Delete one map object by ID
    // ========================================================================
    // Same pattern as PolygonRepository:
    // - DeleteOneAsync with Id filter
    // - Check DeletedCount to see if anything was deleted
    // - Return true if deleted, false if not found
    // ========================================================================
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _mapObjects.DeleteOneAsync(o => o.Id == id);
        return result.DeletedCount > 0;
    }

    // ========================================================================
    // DeleteAllAsync - Delete all map objects
    // ========================================================================
    // Same pattern as PolygonRepository:
    // - DeleteManyAsync with "match all" filter
    // - Clears the entire collection
    // ========================================================================
    public async Task DeleteAllAsync()
    {
        await _mapObjects.DeleteManyAsync(_ => true);
    }
}
