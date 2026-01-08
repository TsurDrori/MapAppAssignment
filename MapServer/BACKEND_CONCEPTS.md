# Backend Concepts Guide for MapServer

This document explains backend programming concepts in plain English. When you see comments in the code that say "See BACKEND_CONCEPTS.md: [Topic]", refer to this document.

---

## Table of Contents

1. [What is a Backend/Server?](#what-is-a-backendserver)
2. [How the Internet Works (HTTP Basics)](#how-the-internet-works-http-basics)
3. [What is an API?](#what-is-an-api)
4. [REST API Explained](#rest-api-explained)
5. [HTTP Methods (GET, POST, DELETE)](#http-methods-get-post-delete)
6. [HTTP Status Codes](#http-status-codes)
7. [What is ASP.NET Core?](#what-is-aspnet-core)
8. [Namespaces](#namespaces)
9. [Classes and Objects](#classes-and-objects)
10. [Properties (get/set)](#properties-getset)
11. [Interfaces](#interfaces)
12. [Dependency Injection](#dependency-injection)
13. [Attributes (Square Brackets)](#attributes-square-brackets)
14. [Async/Await](#asyncawait)
15. [Controllers](#controllers)
16. [Services](#services)
17. [Repositories](#repositories)
18. [DTOs (Data Transfer Objects)](#dtos-data-transfer-objects)
19. [Models](#models)
20. [MongoDB Basics](#mongodb-basics)
21. [The Builder Pattern](#the-builder-pattern)
22. [CORS](#cors)
23. [Lambda Expressions](#lambda-expressions)
24. [Generic Types](#generic-types)
25. [Null Safety](#null-safety)
26. [The Options Pattern](#the-options-pattern)

---

## What is a Backend/Server?

**Simple Analogy**: Think of a restaurant. The frontend (React app) is like the menu and the waiter - it's what customers interact with. The backend is like the kitchen - it does the actual cooking (processing) and stores the ingredients (data).

**What it does**:
- Receives requests from your React app (or any client)
- Processes those requests (validates data, applies business rules)
- Talks to the database to save/retrieve data
- Sends responses back to the client

**Why separate frontend and backend?**
- Security: Sensitive operations (like database access) happen on the server where users can't tamper with them
- Multiple clients: The same backend can serve a website, mobile app, and desktop app
- Scalability: You can run multiple copies of the backend to handle more users

---

## How the Internet Works (HTTP Basics)

**Simple Analogy**: HTTP is like sending letters. Your app (the client) writes a letter (request) with specific instructions, sends it to an address (URL), and waits for a reply (response).

**A request has**:
- **URL**: The address (like `http://localhost:5102/api/polygons`)
- **Method**: What you want to do (GET = read, POST = create, DELETE = remove)
- **Headers**: Extra info (like "I'm sending JSON data")
- **Body**: The actual data (for POST requests)

**A response has**:
- **Status Code**: Was it successful? (200 = yes, 404 = not found, 400 = bad request)
- **Headers**: Extra info about the response
- **Body**: The actual data being returned

---

## What is an API?

**Simple Analogy**: An API is like a menu at a restaurant. It tells you what you can order (endpoints) and what you'll get back. You don't need to know how the kitchen works - you just follow the menu.

**API = Application Programming Interface**

It's a contract that says:
- "If you send THIS request to THIS address..."
- "...I will do THIS and send back THIS response"

Your React app doesn't know or care HOW the server stores polygons in MongoDB. It just knows:
- "Send a GET to /api/polygons" → Get all polygons
- "Send a POST to /api/polygons with polygon data" → Create a polygon

---

## REST API Explained

**REST** is a style/convention for building APIs. It uses standard HTTP methods and URLs in a predictable way.

**The pattern**:
| To do this... | Use this method | To this URL |
|---------------|-----------------|-------------|
| Get ALL polygons | GET | /api/polygons |
| Get ONE polygon | GET | /api/polygons/123 |
| CREATE a polygon | POST | /api/polygons |
| DELETE a polygon | DELETE | /api/polygons/123 |
| DELETE ALL polygons | DELETE | /api/polygons |

**Why REST?**
- It's predictable - once you learn the pattern, you can guess URLs
- It uses existing HTTP features instead of inventing new ones
- It's stateless - each request contains everything needed (the server doesn't "remember" you between requests)

---

## HTTP Methods (GET, POST, DELETE)

| Method | Purpose | Has Request Body? | Example |
|--------|---------|-------------------|---------|
| GET | Read/retrieve data | No | Get all polygons |
| POST | Create new data | Yes | Create a polygon with these coordinates |
| DELETE | Remove data | Usually no | Delete polygon with ID 123 |
| PUT | Replace/update data | Yes | (Not used in this project) |
| PATCH | Partial update | Yes | (Not used in this project) |

**Why no PUT/PATCH in this project?**
The assignment says: Create and Delete only. To "edit" something, delete it and create a new one.

---

## HTTP Status Codes

These are like "result codes" that tell the client what happened.

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 OK | Success | Got data successfully |
| 201 Created | Created successfully | After POST creates something |
| 204 No Content | Success, nothing to return | After DELETE |
| 400 Bad Request | You sent invalid data | Polygon has only 2 coordinates |
| 404 Not Found | That thing doesn't exist | Tried to get polygon with wrong ID |
| 500 Internal Server Error | Server broke | Database connection failed |

---

## What is ASP.NET Core?

**ASP.NET Core** is Microsoft's framework for building web applications and APIs in C#.

Think of it as a "toolkit" that provides:
- A web server to receive HTTP requests
- Routing (matching URLs to code)
- JSON serialization (converting C# objects to JSON and back)
- Dependency injection container
- Configuration management
- And much more...

Without ASP.NET Core, you'd have to write all of this yourself. It's like buying a car vs. building one from scratch.

---

## Namespaces

```csharp
namespace MapServer.DTOs;
```

**Simple Analogy**: Namespaces are like folders on your computer. They organize code and prevent naming conflicts.

**Why use them?**
- You might have a `Polygon` class for API responses (DTO) and a `Polygon` class for database storage (Model)
- By putting them in different namespaces (`MapServer.DTOs.Polygon` vs `MapServer.Models.Polygon`), they don't conflict
- It keeps code organized - you know where to find things

**The convention**: The namespace matches the folder structure
- `MapServer.DTOs` → files in the `DTOs` folder
- `MapServer.Services` → files in the `Services` folder

---

## Classes and Objects

```csharp
public class Coordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

**Simple Analogy**: A class is like a blueprint. An object is a house built from that blueprint.

**Class**: A template that defines what properties and behaviors something has
**Object**: An actual instance created from that template

```csharp
// Coordinate is the class (blueprint)
// coord is an object (actual instance)
var coord = new Coordinate { Latitude = 32.5, Longitude = 35.2 };
```

**`public`**: This class/property can be used from anywhere. There's also:
- `private`: Only usable inside this class
- `protected`: Only usable in this class and its children

---

## Properties (get/set)

```csharp
public double Latitude { get; set; }
```

**What this means**:
- `public` = accessible from outside the class
- `double` = the data type (a decimal number)
- `Latitude` = the name
- `{ get; set; }` = you can both read and write this value

**Why `get; set;` instead of just a variable?**
- It's a C# convention for data containers
- Allows future flexibility (you could add validation later)
- Some frameworks require properties, not fields

**Variations**:
- `{ get; }` = read-only (can only be set in constructor)
- `{ get; private set; }` = readable everywhere, but only this class can change it

---

## Interfaces

```csharp
public interface IPolygonService
{
    Task<List<PolygonDto>> GetAllAsync();
    Task<PolygonDto?> GetByIdAsync(string id);
    // ... more methods
}
```

**Simple Analogy**: An interface is like a job description. It says "anyone who wants this job must be able to do X, Y, and Z". The actual employee (the class) does the work, but the job description (interface) defines what must be done.

**Why use interfaces?**
1. **Swappability**: You could swap `PolygonService` for `MockPolygonService` (for testing) without changing other code
2. **Contracts**: The controller doesn't care HOW the service works, just that it CAN do these things
3. **Testing**: You can create fake implementations for unit tests

**The `I` prefix**: Convention in C# - interfaces start with `I` (IPolygonService, IRepository, etc.)

---

## Dependency Injection

```csharp
// In Program.cs - REGISTRATION
builder.Services.AddScoped<IPolygonService, PolygonService>();

// In PolygonController - USAGE
public PolygonsController(IPolygonService polygonService)
{
    _polygonService = polygonService;
}
```

**Simple Analogy**: Instead of the controller going to the store to buy ingredients (creating its own dependencies), the ingredients are delivered to its door (injected).

**Without Dependency Injection (bad)**:
```csharp
public PolygonsController()
{
    // Controller creates its own dependencies - TIGHT COUPLING
    var context = new MongoDbContext(...);
    var repository = new PolygonRepository(context);
    _service = new PolygonService(repository);
}
```

**With Dependency Injection (good)**:
```csharp
public PolygonsController(IPolygonService service)
{
    _service = service; // Just receives it, doesn't create it
}
```

**Why is this better?**
1. **Testing**: For tests, you can inject a fake service
2. **Flexibility**: Want a different implementation? Change one line in Program.cs
3. **Single Responsibility**: Controller's job is handling HTTP, not creating dependencies

**Lifetimes** (how long objects live):
- `AddSingleton`: ONE instance for the entire application
- `AddScoped`: ONE instance per HTTP request
- `AddTransient`: NEW instance every time it's requested

---

## Attributes (Square Brackets)

```csharp
[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PolygonDto>>> GetAll()
```

**Simple Analogy**: Attributes are like sticky notes attached to code. They provide extra information that the framework reads at runtime.

**Common attributes in this project**:

| Attribute | Meaning |
|-----------|---------|
| `[ApiController]` | "This is an API controller" - enables automatic model validation, etc. |
| `[Route("api/[controller]")]` | "URLs start with /api/polygons" ([controller] becomes the class name minus "Controller") |
| `[HttpGet]` | "This method handles GET requests" |
| `[HttpGet("{id}")]` | "This handles GET requests with an ID in the URL" |
| `[HttpPost]` | "This method handles POST requests" |
| `[HttpDelete]` | "This method handles DELETE requests" |
| `[BsonId]` | "This property is the MongoDB document ID" |
| `[BsonElement("name")]` | "Store this as 'name' in MongoDB" |

---

## Async/Await

```csharp
public async Task<List<PolygonDto>> GetAllAsync()
{
    var polygons = await _repository.GetAllAsync();
    return polygons.Select(MapToDto).ToList();
}
```

**Simple Analogy**: You're making breakfast. While the toast is toasting (async operation), you can make coffee instead of standing there watching. When the toast pops up (await), you go back to handle it.

**Why async?**
- Database operations are SLOW compared to CPU operations
- Without async: Server waits doing NOTHING while database responds
- With async: Server can handle OTHER requests while waiting

**The pattern**:
1. Method is marked `async`
2. Return type is `Task<Something>` instead of just `Something`
3. When calling another async method, use `await`
4. Convention: Async method names end with "Async"

**What happens behind the scenes**:
```csharp
// When await is hit, this thread is freed to handle other requests
var polygons = await _repository.GetAllAsync();
// When database responds, execution continues (possibly on different thread)
```

---

## Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase
```

**What controllers do**:
1. Receive HTTP requests
2. Extract data from the request (URL parameters, body)
3. Call the appropriate service method
4. Return an HTTP response

**Controllers should be THIN** - they shouldn't contain business logic. They're just "traffic directors" that route requests to services.

**Key inherited features from `ControllerBase`**:
- `Ok(data)` - Returns 200 with data
- `NotFound()` - Returns 404
- `BadRequest()` - Returns 400
- `CreatedAtAction(...)` - Returns 201 with location header
- `NoContent()` - Returns 204

---

## Services

```csharp
public class PolygonService : IPolygonService
{
    private readonly IPolygonRepository _repository;

    public PolygonService(IPolygonRepository repository)
    {
        _repository = repository;
    }
```

**What services do**:
1. Contain **business logic** (validation rules, calculations)
2. Coordinate between controllers and repositories
3. Convert between DTOs and Models
4. Make decisions ("is this polygon valid?")

**Why have services?**
- Controllers stay thin and focused on HTTP
- Business logic is reusable (could be called from multiple controllers)
- Easier to test (no HTTP involved, just logic)

---

## Repositories

```csharp
public class PolygonRepository : IPolygonRepository
{
    private readonly IMongoCollection<Polygon> _polygons;
```

**Simple Analogy**: A repository is like a librarian. You ask for a book (data), and the librarian knows where to find it. You don't need to know the library's organization system.

**What repositories do**:
1. Talk directly to the database
2. Convert database results to C# objects
3. Execute queries (find, insert, delete)

**Why have repositories?**
- **Abstraction**: If you switch from MongoDB to PostgreSQL, only repositories change
- **Single Responsibility**: Only one place knows about database operations
- **Testing**: You can mock the repository to test services without a real database

---

## DTOs (Data Transfer Objects)

```csharp
public class PolygonDto
{
    public string? Id { get; set; }
    public List<Coordinate> Coordinates { get; set; } = new();
}
```

**Simple Analogy**: DTOs are like shipping containers. They're designed specifically for transporting data between systems (API ↔ Client).

**Why separate DTOs from Models?**
1. **Security**: You control exactly what data leaves your API
2. **Flexibility**: API contract can differ from database structure
3. **Simplicity**: Client sees simple `Coordinates`, not complex MongoDB `GeoJsonPolygon`

**Naming convention**:
- `PolygonDto` - Response (going OUT to client)
- `CreatePolygonRequest` - Request (coming IN from client)

---

## Models

```csharp
public class Polygon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("geometry")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; } = null!;
}
```

**What models represent**: The actual data structure stored in the database.

**Why different from DTOs?**
- Models have MongoDB-specific attributes (`[BsonId]`, `[BsonElement]`)
- Models use MongoDB data types (`GeoJsonPolygon`)
- DTOs are simpler and database-agnostic

---

## MongoDB Basics

**What is MongoDB?**
A "NoSQL" database that stores data as documents (like JSON) instead of tables.

**Key terms**:
| SQL Term | MongoDB Term |
|----------|--------------|
| Database | Database |
| Table | Collection |
| Row | Document |
| Column | Field |

**A MongoDB document** looks like JSON:
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "geometry": {
    "type": "Polygon",
    "coordinates": [[[35.2, 32.5], [35.3, 32.5], [35.3, 32.6], [35.2, 32.5]]]
  }
}
```

**GeoJSON**: A standard format for geographic data. MongoDB understands it natively and can do spatial queries (find all points inside a polygon, etc.).

---

## The Builder Pattern

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// ... more configuration
var app = builder.Build();
```

**Simple Analogy**: Building a house. First you have a builder (blueprint + plans), you configure everything (number of rooms, paint colors), then you `Build()` to create the actual house.

**Why this pattern?**
- Separates configuration from construction
- You can see all configuration in one place
- Configuration happens once at startup, then the app is "frozen"

---

## CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**What is CORS?**
Cross-Origin Resource Sharing. A security feature in browsers.

**The problem**:
- Your React app runs on `localhost:3000`
- Your API runs on `localhost:5102`
- By default, browsers BLOCK requests between different origins (different ports = different origins)

**The solution**:
- Server explicitly says "I allow requests from localhost:3000"
- Browser sees this and allows the request

**Why does this exist?**
Security. Without CORS, a malicious website could make requests to your bank's API using your logged-in session.

---

## Lambda Expressions

```csharp
// Lambda expression
polygons.Select(p => MapToDto(p))

// Is equivalent to this regular method:
foreach (var p in polygons) {
    yield return MapToDto(p);
}
```

**Simple Analogy**: A lambda is a tiny anonymous function written inline.

**The syntax**: `parameters => expression`
- `p => p.Id` = "given p, return p.Id"
- `(a, b) => a + b` = "given a and b, return a + b"
- `_ => true` = "ignore the parameter, return true" (underscore = unused parameter)

**Common uses**:
```csharp
// Filter: keep only items matching condition
list.Where(x => x.Value > 10)

// Transform: convert each item
list.Select(x => x.Name)

// Find: get first matching item
list.FirstOrDefault(x => x.Id == id)
```

---

## Generic Types

```csharp
GeoJsonPolygon<GeoJson2DGeographicCoordinates>
List<Polygon>
Task<PolygonDto>
```

**Simple Analogy**: Generics are like a "fill in the blank". `List<___>` means "a list of SOMETHING". You fill in what that something is.

**Why generics?**
- `List<int>` = list of integers
- `List<string>` = list of strings
- `List<Polygon>` = list of polygons
- Same code works for any type, but you get type safety

**Common generics you'll see**:
- `List<T>` - A list of items of type T
- `Task<T>` - An async operation that returns type T
- `ActionResult<T>` - An HTTP response containing type T

---

## Null Safety

```csharp
public string? Id { get; set; }        // Can be null
public string ObjectType { get; set; } = string.Empty;  // Cannot be null, has default
public Geometry Geometry { get; set; } = null!;  // Trust me, it won't be null
```

**The `?` symbol**: This value CAN be null
- `string?` = might be null (like before a polygon is saved, it has no ID)
- `string` = should never be null

**The `= value` syntax**: Default value
- `= string.Empty` = if not specified, use empty string
- `= new()` = if not specified, create new empty object
- `= null!` = "I know it looks null, but I promise to set it before using"

**Why care about null?**
Null reference exceptions are one of the most common bugs. Modern C# tries to catch these at compile time.

---

## The Options Pattern

```csharp
// In Program.cs - Configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// In MongoDbContext - Usage
public MongoDbContext(IOptions<MongoDbSettings> settings)
{
    var client = new MongoClient(settings.Value.ConnectionString);
```

**What it does**:
1. Reads settings from `appsettings.json` (or environment variables, etc.)
2. Binds them to a strongly-typed class (`MongoDbSettings`)
3. Injects that class wherever needed

**Why use it?**
- **Type safety**: If you misspell `ConnectionString`, you get a compile error
- **Centralized config**: All settings in one file
- **Environment-specific**: Different settings for dev/staging/production

**The flow**:
```
appsettings.json → Configure<MongoDbSettings> → IOptions<MongoDbSettings> → settings.Value.ConnectionString
```

---

## Architecture Overview

This project follows **Clean Architecture** (also called Onion Architecture):

```
┌─────────────────────────────────────────────┐
│  Controllers (HTTP handling)                │  ← Outermost layer
│  ┌─────────────────────────────────────┐   │
│  │  Services (Business logic)          │   │
│  │  ┌─────────────────────────────┐   │   │
│  │  │  Repositories (Data access)  │   │   │
│  │  │  ┌─────────────────────┐   │   │   │
│  │  │  │  Models (Entities)  │   │   │   │  ← Innermost layer
│  │  │  └─────────────────────┘   │   │   │
│  │  └─────────────────────────────┘   │   │
│  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

**Rules**:
- Inner layers know NOTHING about outer layers
- Dependencies point INWARD
- Each layer has one job

**Why this structure?**
- Easy to test (mock outer layers)
- Easy to change (swap database without changing business logic)
- Easy to understand (each file has one responsibility)

---

## Quick Reference: This Project's Flow

When your React app calls `POST /api/polygons`:

1. **Request arrives** at ASP.NET Core
2. **Routing** matches URL to `PolygonsController.Create()`
3. **Model binding** converts JSON body to `CreatePolygonRequest` object
4. **Controller** calls `_polygonService.CreateAsync(request)`
5. **Service** validates data (at least 3 coordinates)
6. **Service** converts DTO → Model (Coordinate → GeoJsonPolygon)
7. **Service** calls `_repository.CreateAsync(polygon)`
8. **Repository** calls MongoDB driver to insert document
9. **MongoDB** stores the document, returns with generated ID
10. **Repository** returns the `Polygon` model
11. **Service** converts Model → DTO
12. **Controller** returns `CreatedAtAction()` with 201 status
13. **ASP.NET Core** serializes DTO to JSON
14. **Response sent** back to React app
