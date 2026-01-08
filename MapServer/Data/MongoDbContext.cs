// ============================================================================
// MongoDbContext.cs - THE DATABASE CONNECTION MANAGER
// ============================================================================
//
// WHAT: This class manages the connection to MongoDB and provides access
//       to the database collections (tables in SQL terms).
//
// WHY:  We need a central place to:
//       1. Connect to MongoDB (expensive operation - should only do once)
//       2. Get references to our collections (Polygons, MapObjects)
//       3. Create indexes for fast queries
//
// WHEN: This class is created ONCE when the app starts (it's a Singleton).
//       It stays alive for the entire lifetime of the application.
//
// See BACKEND_CONCEPTS.md: MongoDB Basics, Dependency Injection, The Options Pattern
// ============================================================================

// ----------------------------------------------------------------------------
// USING STATEMENTS - Importing code we need
// ----------------------------------------------------------------------------
using MapServer.Configuration;  // For MongoDbSettings class (connection string, etc.)
using MapServer.Models;         // For Polygon and MapObject classes
using Microsoft.Extensions.Options; // For IOptions<T> pattern
using MongoDB.Driver;           // The official MongoDB driver for C#

// This class lives in the MapServer.Data namespace (like a folder)
// Convention: Database-related code goes in a "Data" folder/namespace
namespace MapServer.Data;

// ============================================================================
// MongoDbContext CLASS
// ============================================================================
// "public" = this class can be used from anywhere in the project
// "class" = this is a blueprint for creating objects
//
// This class is registered as a SINGLETON in Program.cs, meaning:
// - Only ONE instance is ever created
// - That instance is reused for every request
// - Perfect for database connections (they're expensive to create)
// ============================================================================
public class MongoDbContext
{
    // ========================================================================
    // PRIVATE FIELD - Internal storage for the database reference
    // ========================================================================
    // "private" = only THIS class can access this field
    // "readonly" = it can only be set in the constructor, then never changed
    // "IMongoDatabase" = the type - an interface representing a MongoDB database
    // "_database" = the name (underscore prefix is C# convention for private fields)
    //
    // WHY PRIVATE? We don't want other code directly accessing the database.
    // They should use the Polygons and MapObjects properties instead.
    // ========================================================================
    private readonly IMongoDatabase _database;

    // ========================================================================
    // CONSTRUCTOR - Runs ONCE when the object is created
    // ========================================================================
    // In C#, the constructor has the same name as the class and no return type.
    //
    // DEPENDENCY INJECTION:
    // - We don't create MongoDbSettings ourselves
    // - ASP.NET Core "injects" it (passes it in) based on Program.cs config
    // - IOptions<MongoDbSettings> is a wrapper that holds our settings
    // - settings.Value gives us the actual MongoDbSettings object
    //
    // See BACKEND_CONCEPTS.md: Dependency Injection, The Options Pattern
    // ========================================================================
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        // ====================================================================
        // STEP 1: Create a MongoDB client (connection to MongoDB server)
        // ====================================================================
        // MongoClient manages the connection pool to MongoDB.
        // The connection string looks like: "mongodb://localhost:27017"
        // This tells the driver WHERE MongoDB is running.
        //
        // WHY CONNECTION STRING? Like a phone number for the database.
        // It can include username, password, server address, port, etc.
        // ====================================================================
        var client = new MongoClient(settings.Value.ConnectionString);

        // ====================================================================
        // STEP 2: Get a reference to our specific database
        // ====================================================================
        // MongoDB can have multiple databases (like having multiple folders).
        // GetDatabase() doesn't actually connect - it just gets a reference.
        // The actual connection happens lazily when we first query.
        //
        // DatabaseName comes from appsettings.json (e.g., "MapServerDb")
        // ====================================================================
        _database = client.GetDatabase(settings.Value.DatabaseName);

        // ====================================================================
        // STEP 3: Get references to our collections
        // ====================================================================
        // Collections in MongoDB are like tables in SQL.
        // GetCollection<T>() says "give me the collection that stores type T"
        //
        // <Polygon> = Generic type parameter - "this collection stores Polygon documents"
        // PolygonsCollectionName = the actual name in MongoDB (e.g., "polygons")
        //
        // See BACKEND_CONCEPTS.md: Generic Types
        // ====================================================================
        Polygons = _database.GetCollection<Polygon>(settings.Value.PolygonsCollectionName);
        MapObjects = _database.GetCollection<MapObject>(settings.Value.ObjectsCollectionName);

        // ====================================================================
        // STEP 4: Create database indexes
        // ====================================================================
        // Indexes make queries faster (like an index in a book).
        // This is called once when the app starts.
        // ====================================================================
        CreateIndexes();
    }

    // ========================================================================
    // PUBLIC PROPERTIES - How other classes access our collections
    // ========================================================================
    // "public" = accessible from outside this class
    // "IMongoCollection<Polygon>" = a collection that stores Polygon documents
    // "{ get; }" = read-only property (no "set" = can't be changed after constructor)
    //
    // WHY PROPERTIES NOT FIELDS?
    // - It's C# convention for exposing data
    // - Properties can have additional logic (validation, lazy loading)
    // - Properties work better with some frameworks
    //
    // These are set in the constructor and then never change.
    // Repositories will use these to query the database.
    //
    // See BACKEND_CONCEPTS.md: Properties (get/set)
    // ========================================================================
    public IMongoCollection<Polygon> Polygons { get; }
    public IMongoCollection<MapObject> MapObjects { get; }

    // ========================================================================
    // CREATE INDEXES - Make geographic queries fast
    // ========================================================================
    // "private" = only this class can call this method
    // "void" = this method doesn't return anything
    //
    // WHY INDEXES?
    // Without an index, MongoDB would scan EVERY document to find matches.
    // With a spatial index, it can quickly find nearby points or shapes.
    //
    // WHAT IS 2dsphere?
    // A special MongoDB index type for geographic data on a sphere (Earth).
    // It understands that longitude 180 and -180 are the same place.
    // It can calculate real distances on Earth's surface.
    // ========================================================================
    private void CreateIndexes()
    {
        // ====================================================================
        // CREATE INDEX ON POLYGON GEOMETRY
        // ====================================================================
        // This creates an index on the "Geometry" field of Polygon documents.
        //
        // BREAKDOWN:
        // - Builders<Polygon> = A helper class for building MongoDB queries/indexes
        // - .IndexKeys = We're building index key definitions
        // - .Geo2DSphere(...) = Create a 2D spherical index (for Earth coordinates)
        // - p => p.Geometry = A lambda saying "index the Geometry field"
        //
        // The lambda "p => p.Geometry" means:
        // "Given a polygon p, we want to index p.Geometry"
        //
        // See BACKEND_CONCEPTS.md: Lambda Expressions
        // ====================================================================
        var polygonIndexKeys = Builders<Polygon>.IndexKeys.Geo2DSphere(p => p.Geometry);

        // Create the index in MongoDB
        // CreateIndexModel wraps the index definition
        // CreateOne() adds it to the database (idempotent - safe to call multiple times)
        Polygons.Indexes.CreateOne(new CreateIndexModel<Polygon>(polygonIndexKeys));

        // ====================================================================
        // CREATE INDEX ON MAP OBJECT LOCATION
        // ====================================================================
        // Same pattern as above, but for MapObject.Location field.
        // This makes it fast to find all objects in a certain area.
        //
        // "o => o.Location" means "index the Location field of each object"
        // ====================================================================
        var objectIndexKeys = Builders<MapObject>.IndexKeys.Geo2DSphere(o => o.Location);
        MapObjects.Indexes.CreateOne(new CreateIndexModel<MapObject>(objectIndexKeys));
    }
}
