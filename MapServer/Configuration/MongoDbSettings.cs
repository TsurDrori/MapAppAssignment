// ============================================================================
// MongoDbSettings.cs - DATABASE CONFIGURATION SETTINGS
// ============================================================================
//
// WHAT: A simple class that holds MongoDB connection settings.
//       It's like a container for configuration values.
//
// WHY:  Instead of hardcoding database settings in code, we store them in
//       appsettings.json. This class maps those JSON values to C# properties.
//
// HOW IT WORKS:
//       1. appsettings.json has a "MongoDbSettings" section with values
//       2. Program.cs calls Configure<MongoDbSettings>(...) to read those values
//       3. This class gets populated with those values
//       4. MongoDbContext receives IOptions<MongoDbSettings> with this data
//
// WHY NOT HARDCODE?
//       - Different values for development vs production
//       - Change settings without recompiling code
//       - Keep secrets out of source code
//
// See BACKEND_CONCEPTS.md: The Options Pattern, Properties (get/set)
// ============================================================================

// This class lives in the MapServer.Configuration namespace
// Convention: Configuration classes go in a "Configuration" folder
namespace MapServer.Configuration;

// ============================================================================
// MongoDbSettings CLASS
// ============================================================================
// This is a "POCO" (Plain Old C# Object) - just a simple data container.
// No logic, no methods, just properties that hold values.
//
// The property names MUST match the JSON keys in appsettings.json:
//
// appsettings.json:
// {
//   "MongoDbSettings": {
//     "ConnectionString": "mongodb://localhost:27017",
//     "DatabaseName": "MapServerDb",
//     "PolygonsCollectionName": "polygons",
//     "ObjectsCollectionName": "objects"
//   }
// }
//
// "public class" = can be accessed from anywhere, is a blueprint for objects
// ============================================================================
public class MongoDbSettings
{
    // ========================================================================
    // ConnectionString - WHERE to find MongoDB
    // ========================================================================
    // Format: "mongodb://[username:password@]host[:port][/database]"
    // Examples:
    //   - "mongodb://localhost:27017" (local development)
    //   - "mongodb://user:pass@server.com:27017" (production with auth)
    //
    // "public" = accessible from outside this class
    // "string" = text data type
    // "{ get; set; }" = can be read and written
    // "= string.Empty" = default value is empty string (avoids null)
    //
    // See BACKEND_CONCEPTS.md: Properties (get/set), Null Safety
    // ========================================================================
    public string ConnectionString { get; set; } = string.Empty;

    // ========================================================================
    // DatabaseName - WHICH database to use
    // ========================================================================
    // MongoDB can have multiple databases. This specifies which one.
    // Example: "MapServerDb"
    //
    // In MongoDB, databases are created automatically when you first use them.
    // You don't need to create them manually.
    // ========================================================================
    public string DatabaseName { get; set; } = string.Empty;

    // ========================================================================
    // PolygonsCollectionName - Name of the polygons collection
    // ========================================================================
    // A "collection" in MongoDB is like a "table" in SQL.
    // This is where polygon documents are stored.
    // Example: "polygons"
    //
    // WHY CONFIGURABLE?
    // - Could have different names in dev vs prod
    // - Allows renaming without code changes
    // - Convention: collection names are lowercase plural
    // ========================================================================
    public string PolygonsCollectionName { get; set; } = string.Empty;

    // ========================================================================
    // ObjectsCollectionName - Name of the map objects collection
    // ========================================================================
    // Where MapObject documents (markers, etc.) are stored.
    // Example: "objects"
    // ========================================================================
    public string ObjectsCollectionName { get; set; } = string.Empty;
}
