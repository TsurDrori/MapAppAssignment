// ============================================================================
// Program.cs - THE ENTRY POINT OF THE APPLICATION
// ============================================================================
//
// WHAT: This file is where the application starts. It runs ONCE when you
//       start the server (dotnet run), and sets everything up.
//
// WHY:  Every .NET application needs an entry point. This file:
//       1. Creates and configures the web server
//       2. Registers all the "services" the app needs (dependency injection)
//       3. Sets up security rules (CORS)
//       4. Starts listening for HTTP requests
//
// See BACKEND_CONCEPTS.md: What is ASP.NET Core?, Dependency Injection, CORS
// ============================================================================

// ----------------------------------------------------------------------------
// USING STATEMENTS - Importing code from other "namespaces" (folders)
// ----------------------------------------------------------------------------
// These are like "import" statements in JavaScript/TypeScript.
// They tell C# "I want to use code from these namespaces"
//
// See BACKEND_CONCEPTS.md: Namespaces
// ----------------------------------------------------------------------------
using MapServer.Configuration;  // Contains MongoDbSettings class
using MapServer.Data;           // Contains MongoDbContext class
using MapServer.Repositories;   // Contains IPolygonRepository, PolygonRepository, etc.
using MapServer.Services;       // Contains IPolygonService, PolygonService, etc.

// ============================================================================
// BUILDER PHASE - Configure the application BEFORE it runs
// ============================================================================
// The "builder" pattern: first we configure everything, then we "build" the app.
// Think of it like ordering a custom computer - you specify all the parts first,
// then the factory builds it.
//
// See BACKEND_CONCEPTS.md: The Builder Pattern
// ============================================================================

// This creates a "builder" object that we'll use to configure the application.
// "args" are command-line arguments passed when you run "dotnet run"
// (we don't use them in this project, but they're standard to include)
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// MONGODB CONFIGURATION - Tell the app how to connect to the database
// ============================================================================
// This reads settings from appsettings.json (or appsettings.Development.json)
// and maps them to the MongoDbSettings class.
//
// HOW IT WORKS:
// 1. appsettings.json has a section called "MongoDbSettings" with ConnectionString, etc.
// 2. Configure<MongoDbSettings>() reads that section
// 3. Wherever we need these settings, we ask for IOptions<MongoDbSettings>
//
// See BACKEND_CONCEPTS.md: The Options Pattern
// ============================================================================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
// "GetSection" finds the "MongoDbSettings" section in appsettings.json
// "Configure<MongoDbSettings>" binds those JSON values to the C# class properties

// ============================================================================
// FRAMEWORK SERVICES - Add built-in ASP.NET Core features
// ============================================================================

// AddControllers():
// - Tells ASP.NET Core "this app uses Controllers for handling HTTP requests"
// - Enables routing (matching URLs to controller methods)
// - Enables automatic JSON serialization (converting C# objects to JSON for responses)
// - Without this, the [ApiController] and [HttpGet] attributes wouldn't work
builder.Services.AddControllers();

// AddOpenApi():
// - Enables OpenAPI/Swagger documentation
// - Creates a machine-readable description of your API at /openapi/v1.json
// - Useful for testing and generating client code
// - Only used in development (see app.MapOpenApi() below)
builder.Services.AddOpenApi();

// ============================================================================
// CORS CONFIGURATION - Allow your React app to call this API
// ============================================================================
// Browsers block cross-origin requests by default (security feature).
// Since React runs on localhost:3000 or :5173, and API runs on :5102,
// they are "different origins" and the browser would block the requests.
//
// This configuration tells the browser: "It's OK, let these origins through"
//
// See BACKEND_CONCEPTS.md: CORS
// ============================================================================
builder.Services.AddCors(options =>                      // Configure CORS
{
    options.AddPolicy("AllowReactApp", policy =>         // Create a named policy
    {
        policy.WithOrigins(                              // These URLs are allowed:
                "http://localhost:3000",                 // Create React App default port
                "http://localhost:5173"                  // Vite default port
            )
            .AllowAnyHeader()                            // Allow any HTTP headers
            .AllowAnyMethod();                           // Allow GET, POST, DELETE, etc.
    });
});

// ============================================================================
// DEPENDENCY INJECTION REGISTRATION - "Here's how to create these objects"
// ============================================================================
// When a class says "I need an IPolygonService", ASP.NET Core needs to know
// HOW to create one. These lines tell it:
// - "When someone asks for IPolygonService, give them a PolygonService"
//
// See BACKEND_CONCEPTS.md: Dependency Injection
// ============================================================================

// SINGLETON: Only ONE instance is created for the ENTIRE application lifetime.
// MongoDbContext is a singleton because:
// - Database connections are expensive to create
// - We only need one connection pool shared by everyone
// - It holds the database collections (Polygons, MapObjects)
builder.Services.AddSingleton<MongoDbContext>();

// SCOPED: One instance is created PER HTTP REQUEST.
// Repositories and Services are scoped because:
// - Each request should get its own fresh instance
// - Scoped services can depend on other scoped services
// - Memory is freed after the request completes

// Register repositories (data access layer)
// "When someone asks for IPolygonRepository, create a PolygonRepository"
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<IMapObjectRepository, MapObjectRepository>();

// Register services (business logic layer)
// "When someone asks for IPolygonService, create a PolygonService"
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IMapObjectService, MapObjectService>();

// ============================================================================
// BUILD THE APP - Done configuring, now create the actual application
// ============================================================================
// This takes all the configuration we did above and creates a runnable app.
// After this line, the app is configured but NOT YET running.
var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE - Configure how requests are processed
// ============================================================================
// "Middleware" is code that runs for EVERY request, in order.
// Think of it like an assembly line - each piece of middleware does one thing.
//
// Request comes in → Middleware 1 → Middleware 2 → Controller → Response goes out
// ============================================================================

// Development-only features
// IsDevelopment() returns true when running locally (based on ASPNETCORE_ENVIRONMENT)
if (app.Environment.IsDevelopment())
{
    // MapOpenApi(): Makes the API documentation available at /openapi/v1.json
    // Only in development because production APIs often don't expose this
    app.MapOpenApi();
}

// UseHttpsRedirection():
// - If someone requests http://..., redirect them to https://...
// - Security best practice - HTTPS encrypts the connection
// - In production, this ensures all traffic is encrypted
app.UseHttpsRedirection();

// UseCors("AllowReactApp"):
// - Applies the CORS policy we defined earlier
// - This MUST be called before MapControllers() to work
// - The string "AllowReactApp" matches the policy name we defined above
app.UseCors("AllowReactApp");

// MapControllers():
// - Tells ASP.NET Core "look at all the [Route] attributes and set up URL routing"
// - This scans all Controller classes and registers their endpoints
// - After this, /api/polygons will route to PolygonsController, etc.
app.MapControllers();

// ============================================================================
// START THE SERVER - Begin listening for HTTP requests
// ============================================================================
// app.Run() starts the web server and blocks (waits) until the app is stopped.
// This is the last line because nothing after it would run while the server is up.
//
// After this runs, the console shows:
// "Now listening on: https://localhost:7103"
// "Now listening on: http://localhost:5102"
//
// The app keeps running until you press Ctrl+C or close the terminal.
// ============================================================================
app.Run();
