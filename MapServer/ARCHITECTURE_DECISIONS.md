# MapServer Architecture Decisions - A Deep Dive

**A comprehensive guide to the architectural choices made in building a geospatial REST API**

*Target Audience: Developers learning about API design, Clean Architecture, and MongoDB integration*

---

## Table of Contents
1. [Project Context](#project-context)
2. [Architectural Pattern Selection](#architectural-pattern-selection)
3. [API Design Decisions](#api-design-decisions)
4. [Data Model Strategy](#data-model-strategy)
5. [Technology Choices](#technology-choices)
6. [Validation Strategy](#validation-strategy)
7. [Error Handling Approach](#error-handling-approach)
8. [Database Design](#database-design)

---

## Project Context

### The Assignment
Build a client-server map application where users can:
- Draw and manage polygons on a map
- Place objects (markers/custom symbols) on the map
- Store and retrieve this data persistently

### Key Requirements
- **Client**: React with Leaflet/MapLibre
- **Server**: ASP.NET Core REST API
- **Database**: MongoDB
- **Operations**: Create and Delete only (no Update specified)

---

## 1. Architectural Pattern Selection

### The Decision: Clean Architecture (Onion Architecture)

We chose to implement Clean Architecture with four distinct layers:

```
┌─────────────────────────────┐
│   Presentation Layer        │  Controllers (HTTP concerns)
│   (Controllers/)            │
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│   Application Layer         │  Services, DTOs, Interfaces
│   (Services/, DTOs/)        │  (Business Logic)
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│   Domain Layer              │  Entities, Value Objects
│   (Models/)                 │  (Business Rules)
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│   Infrastructure Layer      │  MongoDB Repositories
│   (Repositories/, Data/)    │  (External Concerns)
└─────────────────────────────┘
```

### Options Considered

#### Option A: No Architecture (Controllers + MongoDB Direct)
```csharp
[ApiController]
public class PolygonsController : ControllerBase {
    private readonly IMongoCollection<Polygon> _collection;

    [HttpPost]
    public async Task<IActionResult> Create(Polygon polygon) {
        await _collection.InsertOneAsync(polygon);
        return Ok(polygon);
    }
}
```

**Pros:**
- ✅ Fastest to implement (minimal code)
- ✅ No abstraction layers to learn
- ✅ Good for prototypes and MVPs

**Cons:**
- ❌ Controllers become bloated with business logic
- ❌ Impossible to unit test without database
- ❌ Tight coupling to MongoDB (hard to switch databases)
- ❌ Validation logic mixed with HTTP concerns
- ❌ Code duplication across controllers

**Why We Rejected It:**
For an interview assignment, demonstrating architectural knowledge is crucial. While this approach works for small apps, it doesn't scale and shows poor separation of concerns.

---

#### Option B: MVC with Repository Pattern
```
Controllers/
  PolygonsController.cs
  ObjectsController.cs
Repositories/
  PolygonRepository.cs
  ObjectRepository.cs
Models/
  Polygon.cs
  MapObject.cs
```

**Pros:**
- ✅ Separates data access from presentation
- ✅ Testable with repository mocking
- ✅ Familiar pattern for most developers
- ✅ Simpler than Clean Architecture

**Cons:**
- ⚠️ Business logic often ends up in controllers (fat controllers)
- ⚠️ No clear place for DTOs (often use entities as API contracts)
- ⚠️ Validation scattered between controllers and repositories
- ⚠️ Domain models exposed to API clients

**Why We Rejected It:**
While adequate, it doesn't provide a clear home for business logic and validation. For a slightly larger app, we'd need to refactor anyway.

---

#### Option C: Clean Architecture (Our Choice)
```
Controllers/        → HTTP routing only
Services/          → Business logic + validation
Repositories/      → Data access
Models/            → Domain entities
DTOs/              → API contracts
```

**Pros:**
- ✅ **Clear separation of concerns**: Each layer has one job
- ✅ **Highly testable**: Services can be tested independently
- ✅ **Technology agnostic**: Can swap MongoDB for SQL Server
- ✅ **Explicit dependencies**: Interfaces define contracts
- ✅ **Professional standard**: Industry best practice
- ✅ **Scalable**: Easy to add features without breaking existing code

**Cons:**
- ⚠️ More files and folders to manage
- ⚠️ Steeper learning curve for beginners
- ⚠️ Slight over-engineering for a 2-entity app
- ⚠️ More boilerplate code

**Why We Chose It:**
1. **Interview Context**: Demonstrates architectural knowledge
2. **Maintainability**: Clear structure makes code easy to navigate
3. **Testability**: Each layer can be tested independently
4. **Professional Standards**: Shows understanding of enterprise patterns

### Layer Responsibilities

#### Presentation Layer (Controllers)
**Responsibility**: Handle HTTP concerns ONLY
- Route requests to correct service methods
- Convert HTTP requests to DTOs
- Convert service results to HTTP responses (200, 400, 404, 500)
- Handle exceptions and return appropriate status codes

**What NOT to put here:**
- ❌ Business logic
- ❌ Validation (except basic model binding)
- ❌ Database queries
- ❌ Data transformation

```csharp
// GOOD - Thin controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePolygonDto dto) {
    try {
        var polygon = await _polygonService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = polygon.Id }, polygon);
    } catch (ValidationException ex) {
        return BadRequest(ex.Message);
    }
}

// BAD - Fat controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePolygonDto dto) {
    // Validation in controller ❌
    if (dto.Coordinates.Count < 3) return BadRequest("Need 3+ coords");

    // Business logic in controller ❌
    if (!IsClosedPolygon(dto.Coordinates)) {
        dto.Coordinates.Add(dto.Coordinates.First());
    }

    // Direct database access ❌
    var polygon = new Polygon { Coordinates = dto.Coordinates };
    await _mongoCollection.InsertOneAsync(polygon);

    return Ok(polygon);
}
```

#### Application Layer (Services)
**Responsibility**: Orchestrate business logic and validation
- Validate input data
- Apply business rules
- Coordinate between repositories
- Transform DTOs to domain entities
- Ensure data integrity

```csharp
public class PolygonService : IPolygonService {
    private readonly IPolygonRepository _repository;

    public async Task<PolygonDto> CreateAsync(CreatePolygonDto dto) {
        // Validation
        if (dto.Coordinates.Count < 3) {
            throw new ValidationException("Polygon must have at least 3 coordinates");
        }

        // Business rule: auto-close polygon
        var coordinates = EnsureClosedPolygon(dto.Coordinates);

        // Delegate to repository
        var polygon = await _repository.CreateAsync(coordinates);

        // Transform to DTO
        return MapToDto(polygon);
    }
}
```

#### Domain Layer (Models)
**Responsibility**: Define core business entities
- Plain C# objects (POCOs)
- No dependencies on infrastructure
- Represent business concepts

```csharp
public class Polygon {
    public string Id { get; set; }
    public List<Coordinate> Coordinates { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Coordinate {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

#### Infrastructure Layer (Repositories)
**Responsibility**: Handle data persistence
- Convert domain models to database models
- Execute database queries
- Handle connection management
- Return domain models (not database-specific types)

```csharp
public class PolygonRepository : IPolygonRepository {
    private readonly IMongoCollection<PolygonDocument> _collection;

    public async Task<Polygon> CreateAsync(List<Coordinate> coordinates) {
        // Convert to MongoDB document
        var document = new PolygonDocument {
            Geometry = ToGeoJsonPolygon(coordinates)
        };

        await _collection.InsertOneAsync(document);

        // Convert back to domain model
        return ToDomainModel(document);
    }
}
```

---

## 2. API Design Decisions

### Decision 1: Batch vs Single Object Creation

**The Requirement:**
Assignment states: "Sends the **list of objects** to the server" (line 21)

### Options Explored

#### Option A: Single Endpoint (Accept Array OR Single Object)
```csharp
POST /api/objects
Body: { "lat": 40.7, "lon": -74.0, "type": "Marker" }
  OR
Body: [{ "lat": 40.7, "lon": -74.0, "type": "Marker" }, ...]
```

**Pros:**
- ✅ Single endpoint to document
- ✅ Flexible for client

**Cons:**
- ❌ **Non-standard REST**: Endpoints should have consistent contracts
- ❌ Confusing API - clients don't know which to use
- ❌ Harder to version if needs diverge
- ❌ Complex validation (array vs object)

**Implementation Complexity:**
```csharp
public async Task<IActionResult> Create([FromBody] JsonElement json) {
    if (json.ValueKind == JsonValueKind.Array) {
        // Handle array
        var objects = JsonSerializer.Deserialize<List<CreateObjectDto>>(json);
        return await CreateBatch(objects);
    } else {
        // Handle single
        var obj = JsonSerializer.Deserialize<CreateObjectDto>(json);
        return await CreateSingle(obj);
    }
}
```

**Why We Rejected It:**
API contracts should be explicit and predictable. Accepting two different types violates the principle of least surprise.

---

#### Option B: Single Endpoint Only
```csharp
POST /api/objects
Body: { "lat": 40.7, "lon": -74.0, "type": "Marker" }
```

**Pros:**
- ✅ RESTful standard (POST creates one resource)
- ✅ Simple, predictable
- ✅ Easy to understand and document

**Cons:**
- ❌ **Violates assignment**: Says "list of objects"
- ❌ Multiple HTTP requests for multiple objects
- ❌ Network overhead (round-trips, headers per request)
- ❌ No transactional batch creation

**Network Cost Example:**
```
Creating 10 objects:
- 10 HTTP requests
- 10 × ~200 bytes headers = 2KB overhead
- 10 × TCP handshake latency
- Total time: ~500ms (vs ~50ms for batch)
```

**Why We Rejected It:**
Doesn't match assignment requirements. While RESTfully correct, it's inefficient for the use case.

---

#### Option C: Separate Endpoints (Our Choice)
```csharp
POST /api/objects        → Single object
POST /api/objects/batch  → Array of objects
```

**Pros:**
- ✅ **Matches assignment**: Batch endpoint for "list of objects"
- ✅ **RESTful**: Single endpoint follows POST standards
- ✅ **Explicit contracts**: Clear what each endpoint does
- ✅ **Flexible**: Client chooses based on use case
- ✅ **Efficient**: Batch reduces network overhead
- ✅ **Independent evolution**: Can add batch-specific features (partial failure handling)

**Cons:**
- ⚠️ Two endpoints to maintain
- ⚠️ Slight code duplication (mitigated by shared service)

**Implementation:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateObjectDto dto) {
    var result = await _service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

[HttpPost("batch")]
public async Task<IActionResult> CreateBatch([FromBody] List<CreateObjectDto> dtos) {
    var results = await _service.CreateBatchAsync(dtos);
    return Ok(results);
}
```

**Why We Chose It:**
1. **Assignment compliance**: Explicitly supports "list of objects"
2. **Best practices**: Separate endpoints for different behaviors
3. **Performance**: Batch operations are network-efficient
4. **Clarity**: No ambiguity about what each endpoint does

---

### Decision 2: Delete All vs Delete Single

**The Requirement:**
- Polygons: "Deletes a selected polygon **or all polygons**"
- Objects: "Removes the **selected object**" (no "all" mentioned)

### Options Explored

#### Option A: Literal Interpretation (Asymmetric)
```
DELETE /api/polygons/{id}    ✅
DELETE /api/polygons         ✅
DELETE /api/objects/{id}     ✅
DELETE /api/objects          ❌ Not implemented
```

**Pros:**
- ✅ Follows assignment exactly
- ✅ Less code to write

**Cons:**
- ❌ **Inconsistent API**: Why can you delete all polygons but not all objects?
- ❌ **Poor UX**: User expects "Clear Map" button to clear everything
- ❌ **Workaround needed**: Client must fetch all IDs then delete individually
- ❌ **Unprofessional**: Interview context - shows lack of API design thinking

**Client Impact:**
```javascript
// Without DELETE all objects - clunky
const objects = await fetch('/api/objects').then(r => r.json());
await Promise.all(objects.map(o => fetch(`/api/objects/${o.id}`, { method: 'DELETE' })));

// With DELETE all objects - clean
await fetch('/api/objects', { method: 'DELETE' });
```

---

#### Option B: Symmetric API (Our Choice)
```
DELETE /api/polygons/{id}    ✅
DELETE /api/polygons         ✅
DELETE /api/objects/{id}     ✅
DELETE /api/objects          ✅ Added for consistency
```

**Pros:**
- ✅ **Consistent**: Both resources have same capabilities
- ✅ **Predictable**: Developer intuition about one applies to both
- ✅ **Better UX**: "Clear Map" button can clear all with 2 requests
- ✅ **Professional**: Shows thoughtful API design
- ✅ **RESTful**: Collection endpoint (no ID) = operate on collection

**Cons:**
- ⚠️ Goes beyond literal assignment (but improves design)
- ⚠️ Slightly more code (~10 lines)

**Why We Chose It:**
In an interview context, demonstrating good judgment to improve on requirements is valuable. The asymmetry would likely come up in code review as an inconsistency.

---

### Decision 3: Update Operations (PUT/PATCH)

**The Requirement:**
Assignment specifies: "Add / Save / Delete" - no Update/Edit mentioned

### Options Explored

#### Option A: Implement Full CRUD
```
POST   /api/polygons      Create
GET    /api/polygons/{id} Read
PUT    /api/polygons/{id} Update (full replacement)
PATCH  /api/polygons/{id} Update (partial)
DELETE /api/polygons/{id} Delete
```

**Pros:**
- ✅ Complete REST API
- ✅ Professional, production-ready
- ✅ Better user experience (edit without delete)

**Cons:**
- ❌ **Not in assignment**: Adds scope beyond requirements
- ❌ More code to write/test (~30% more)
- ❌ **Interview time constraint**: Focus on what's asked
- ❌ Need to handle partial updates (PATCH) complexity

**Implementation Effort:**
```csharp
// PUT endpoint
[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, UpdatePolygonDto dto) {
    var polygon = await _service.UpdateAsync(id, dto);
    return Ok(polygon);
}

// Service validation
public async Task<Polygon> UpdateAsync(string id, UpdatePolygonDto dto) {
    var existing = await _repository.GetByIdAsync(id);
    if (existing == null) throw new NotFoundException();

    // Validate new coordinates
    if (dto.Coordinates.Count < 3) throw new ValidationException();

    // Update
    existing.Coordinates = dto.Coordinates;
    await _repository.UpdateAsync(existing);
    return existing;
}
```

---

#### Option B: Omit Updates (Our Choice)
**Current API:**
- Create ✅
- Read ✅
- Update ❌
- Delete ✅

**Workaround for edits:**
```javascript
// Client can delete + recreate to "update"
await fetch(`/api/polygons/${id}`, { method: 'DELETE' });
await fetch('/api/polygons', {
    method: 'POST',
    body: JSON.stringify(updatedPolygon)
});
```

**Pros:**
- ✅ **Follows assignment**: Implements exactly what's specified
- ✅ **Simpler**: Less code, less testing
- ✅ **Time efficient**: Focus on core requirements
- ✅ **Extensible**: Architecture supports easy addition later

**Cons:**
- ⚠️ Incomplete CRUD (but assignment doesn't require it)
- ⚠️ Clunky edit UX (but that's the assignment design)
- ⚠️ Loss of metadata (creation timestamps) on edit

**Why We Chose It:**
1. **Interview context**: Demonstrate you can follow requirements
2. **Time management**: Don't gold-plate
3. **Clean Architecture**: Makes adding PUT later trivial if needed
4. **Can discuss in interview**: "In production, I'd add PUT for better UX"

---

## 3. Data Model Strategy

### Decision: DTO Pattern vs Direct Entity Exposure

One of the most critical architectural decisions: **Should API clients work with domain entities directly, or use Data Transfer Objects (DTOs)?**

### The Problem

Our domain entities need to work with MongoDB's GeoJSON types:
```csharp
public class Polygon {
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; }
}
```

But our API clients (React app) work with simple coordinates:
```json
{
  "coordinates": [
    { "latitude": 40.7128, "longitude": -74.0060 },
    { "latitude": 40.7580, "longitude": -73.9855 }
  ]
}
```

**The tension:** MongoDB needs GeoJSON, clients need simplicity. Where does conversion happen?

### Options Explored

#### Option A: Expose MongoDB Types in API (No DTOs)
```csharp
// Controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] Polygon polygon) {
    await _repository.CreateAsync(polygon);
    return Ok(polygon);
}

// Client must send
{
  "geometry": {
    "type": "Polygon",
    "coordinates": [[
      [-74.0060, 40.7128],  // Note: [lon, lat] order!
      [-73.9855, 40.7580],
      [-74.0060, 40.7128]
    ]]
  }
}
```

**Pros:**
- ✅ No mapping code needed
- ✅ Fewer classes to maintain
- ✅ Direct database model = API model

**Cons:**
- ❌ **Couples API to MongoDB**: Changing DB requires API breaking change
- ❌ **Complex client code**: React must construct GeoJSON structures
- ❌ **Leaky abstraction**: Internal implementation details exposed
- ❌ **Coordinate order confusion**: GeoJSON uses [lon, lat], Leaflet uses [lat, lon]
- ❌ **Untestable business logic**: Services tied to infrastructure types
- ❌ **Violates Clean Architecture**: Domain depends on infrastructure

**Real-World Problem:**
If you switch from MongoDB to PostGIS, the API contract breaks:
```csharp
// MongoDB
public GeoJsonPolygon<...> Geometry { get; set; }

// PostGIS uses different types
public NetTopologySuite.Geometries.Polygon Geometry { get; set; }

// All clients break! ❌
```

---

#### Option B: DTOs with Manual Mapping
```csharp
// Simple DTO
public class CreatePolygonDto {
    public List<CoordinateDto> Coordinates { get; set; }
}

public class CoordinateDto {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

// Domain Entity
public class Polygon {
    public string Id { get; set; }
    public List<Coordinate> Coordinates { get; set; }
}

// MongoDB Document (internal to repository)
internal class PolygonDocument {
    public ObjectId Id { get; set; }
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; }
}

// Mapping in Repository
public async Task<Polygon> CreateAsync(List<Coordinate> coordinates) {
    var doc = new PolygonDocument {
        Geometry = new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(
                new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(
                    coordinates.Select(c => new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude))
                )
            )
        )
    };

    await _collection.InsertOneAsync(doc);
    return ToDomainModel(doc);
}
```

**Pros:**
- ✅ **Clean API contract**: Simple, technology-agnostic
- ✅ **Database independence**: Swap MongoDB without breaking API
- ✅ **Testable**: Services work with POCOs, not infrastructure types
- ✅ **Clear boundaries**: Each layer has appropriate models
- ✅ **Client-friendly**: Natural JSON structure

**Cons:**
- ⚠️ Mapping code (but isolated in one place)
- ⚠️ More classes (DTOs + Entities + Documents)
- ⚠️ Coordinate conversion logic needed

**Why We Chose It:**
1. **Clean Architecture principle**: Domain shouldn't depend on infrastructure
2. **API stability**: Technology changes don't break contracts
3. **Testability**: Can test business logic without MongoDB
4. **Professional standard**: Industry best practice for layered apps

---

#### Option C: AutoMapper
```csharp
// Configuration
CreateMap<CreatePolygonDto, Polygon>();
CreateMap<Polygon, PolygonDto>();

// Usage
var polygon = _mapper.Map<Polygon>(dto);
```

**Pros:**
- ✅ Less boilerplate mapping code
- ✅ Convention-based configuration
- ✅ Widely used in enterprise

**Cons:**
- ❌ **Overkill for simple project**: 2 entities don't justify dependency
- ❌ Magic conventions can hide bugs
- ❌ Complex GeoJSON mapping still needs custom resolvers
- ❌ Runtime errors if misconfigured

**Why We Rejected It:**
For a small project with custom GeoJSON conversion logic, explicit mapping is clearer and doesn't require learning AutoMapper's API.

---

### The Three-Model Pattern (Our Choice)

We use **three separate model types** for different concerns:

```
┌─────────────────┐
│  CreatePolygonDto│  ← Controller receives
│  PolygonDto      │  ← Controller returns
└────────┬─────────┘
         │
         │ Service maps
         ▼
┌─────────────────┐
│  Polygon        │  ← Domain entity (business logic)
│  Coordinate     │
└────────┬─────────┘
         │
         │ Repository maps
         ▼
┌─────────────────┐
│ PolygonDocument │  ← MongoDB document (internal)
│ GeoJsonPolygon  │
└─────────────────┘
```

**Layer 1: DTOs (API Contracts)**
```csharp
// What the API accepts
public class CreatePolygonDto {
    public List<CoordinateDto> Coordinates { get; set; }
}

// What the API returns
public class PolygonDto {
    public string Id { get; set; }
    public List<CoordinateDto> Coordinates { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CoordinateDto {
    public double Latitude { get; set; }  // Leaflet order: lat, lon
    public double Longitude { get; set; }
}
```

**Benefits:**
- Client-friendly JSON structure
- Can evolve independently from domain
- Can have different validation attributes than domain

**Layer 2: Domain Models (Business Logic)**
```csharp
public class Polygon {
    public string Id { get; set; }
    public List<Coordinate> Coordinates { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsClosed() {
        return Coordinates.First().Equals(Coordinates.Last());
    }
}

public class Coordinate {
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public override bool Equals(object obj) {
        var other = obj as Coordinate;
        return other != null &&
               Math.Abs(Latitude - other.Latitude) < 0.0001 &&
               Math.Abs(Longitude - other.Longitude) < 0.0001;
    }
}
```

**Benefits:**
- Pure C# objects (POCOs)
- Can have business logic methods
- No infrastructure dependencies
- Easy to unit test

**Layer 3: MongoDB Documents (Persistence)**
```csharp
internal class PolygonDocument {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("geometry")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
```

**Benefits:**
- Optimized for MongoDB storage
- Can use 2dsphere indexes for geo queries
- Hidden from other layers (internal class)
- Can change without affecting domain/API

---

### Coordinate Order: A Critical Decision

**The Problem:** GeoJSON and Leaflet use OPPOSITE coordinate orders!

- **GeoJSON standard**: `[Longitude, Latitude]` - because it's `[X, Y]` on a map
- **Leaflet library**: `[Latitude, Longitude]` - because it's more natural to humans

### Options Explored

#### Option A: API Uses GeoJSON Order [Lon, Lat]
```json
{
  "coordinates": [
    [-74.0060, 40.7128],
    [-73.9855, 40.7580]
  ]
}
```

**Pros:**
- ✅ Matches database format
- ✅ GeoJSON standard
- ✅ No server-side conversion

**Cons:**
- ❌ **Confusing for developers**: Latitude is more commonly first
- ❌ **Client must convert**: Leaflet uses [lat, lon]
- ❌ **Bug-prone**: Easy to swap accidentally

---

#### Option B: API Uses Natural Order [Lat, Lon] (Our Choice)
```json
{
  "coordinates": [
    { "latitude": 40.7128, "longitude": -74.0060 },
    { "latitude": 40.7580, "longitude": -73.9855 }
  ]
}
```

**Pros:**
- ✅ **Explicit**: Named properties prevent confusion
- ✅ **Natural**: Matches how humans think (latitude first)
- ✅ **Matches Leaflet**: Direct mapping to client library
- ✅ **Type-safe**: Can't accidentally swap values

**Cons:**
- ⚠️ Server must convert to GeoJSON [lon, lat] for MongoDB

**Conversion Logic:**
```csharp
// Repository handles conversion
private GeoJsonPolygon<GeoJson2DGeographicCoordinates> ToGeoJson(List<Coordinate> coords) {
    var geoCoords = coords.Select(c =>
        new GeoJson2DGeographicCoordinates(
            c.Longitude,  // GeoJSON: longitude first!
            c.Latitude
        )
    );
    return new GeoJsonPolygon<...>(
        new GeoJsonPolygonCoordinates<...>(
            new GeoJsonLinearRingCoordinates<...>(geoCoords)
        )
    );
}
```

**Why We Chose It:**
1. **Developer experience**: Explicit properties prevent bugs
2. **Client convenience**: Matches Leaflet's convention
3. **Encapsulation**: Conversion complexity hidden in repository layer

---

## 4. Technology Choices

### Decision 1: MongoDB vs SQL Database

**The Requirement:** Assignment specifies MongoDB

### Why MongoDB Makes Sense Here

**Native Geospatial Support:**
```javascript
// MongoDB 2dsphere index enables powerful geo queries
db.polygons.createIndex({ geometry: "2dsphere" });

// Find polygons containing a point
db.polygons.find({
  geometry: {
    $geoIntersects: {
      $geometry: { type: "Point", coordinates: [-74.0060, 40.7128] }
    }
  }
});

// Find objects within 5km of location
db.objects.find({
  location: {
    $near: {
      $geometry: { type: "Point", coordinates: [-74.0060, 40.7128] },
      $maxDistance: 5000
    }
  }
});
```

**Flexible Schema:**
```json
// Can easily add new fields without migrations
{
  "_id": "...",
  "geometry": { "type": "Polygon", "coordinates": [...] },
  "properties": {
    "name": "Central Park",
    "category": "park",
    "tags": ["public", "recreation"]
  }
}
```

**Comparison with SQL (PostGIS):**

| Feature | MongoDB | PostgreSQL + PostGIS |
|---------|---------|---------------------|
| Geo queries | Native 2dsphere | Extension required |
| Setup complexity | Low | Medium (install PostGIS) |
| JSON storage | Native BSON | JSONB type |
| Schema flexibility | Schemaless | Rigid schema + migrations |
| Geo indexing | Automatic | Manual tuning |
| Learning curve | Low | Higher (SQL + extensions) |

**For This Project:**
- ✅ Assignment requirement
- ✅ Simple setup
- ✅ Good match for geospatial data
- ✅ No complex joins needed

---

### Decision 2: ASP.NET Core 10 vs Other Frameworks

**The Requirement:** Assignment specifies ASP.NET Core

### Why ASP.NET Core is Excellent Choice

**Performance:**
```
TechEmpower Benchmark (Round 21)
Requests/sec:
- ASP.NET Core: 7,052,336
- Node.js Express: 49,967
- Spring Boot: 178,000

ASP.NET Core is one of the fastest web frameworks.
```

**Built-in Features:**
```csharp
// Dependency Injection - built-in
services.AddScoped<IPolygonService, PolygonService>();

// API Documentation - built-in
services.AddOpenApi();

// Model Validation - built-in
public class CreatePolygonDto {
    [Required]
    [MinLength(3)]
    public List<CoordinateDto> Coordinates { get; set; }
}

// Async/await - language-level support
public async Task<Polygon> CreateAsync(CreatePolygonDto dto) { ... }
```

**Cross-Platform:**
- Runs on Windows, Linux, macOS
- Docker support out of box
- Cloud-ready (Azure, AWS, GCP)

**Type Safety:**
- Compile-time error checking
- IntelliSense support
- Refactoring safety

**Compared to Alternatives:**

| Framework | Pros | Cons |
|-----------|------|------|
| **ASP.NET Core** | Fast, type-safe, great tooling | Requires C# knowledge |
| Node.js/Express | Simple, JavaScript everywhere | Slower, no type safety (without TS) |
| Python/FastAPI | Rapid development, clean syntax | Slower than compiled languages |
| Go/Gin | Extremely fast, simple deployment | Verbose error handling, less tooling |
| Spring Boot (Java) | Enterprise standard, mature | Heavy, slower startup |

---

### Decision 3: MongoDB.Driver Version 3.5.2

**Why This Version:**
- ✅ Latest stable (as of project start)
- ✅ Native GeoJSON support
- ✅ Async/await support
- ✅ LINQ integration

**Key Features Used:**
```csharp
// 1. GeoJSON Types
GeoJsonPolygon<GeoJson2DGeographicCoordinates>
GeoJsonPoint<GeoJson2DGeographicCoordinates>

// 2. Async Operations
await collection.InsertOneAsync(document);
await collection.Find(filter).ToListAsync();

// 3. LINQ Queries
var polygons = await collection
    .Find(p => p.CreatedAt > DateTime.UtcNow.AddDays(-7))
    .ToListAsync();

// 4. Indexes
await collection.Indexes.CreateOneAsync(
    new CreateIndexModel<PolygonDocument>(
        Indexes.Geo2DSphere(doc => doc.Geometry)
    )
);
```

---

## 5. Validation Strategy

### The Decision: Service-Level Validation vs Separate Validators

### Options Explored

#### Option A: FluentValidation Library
```csharp
// Separate validator class
public class CreatePolygonDtoValidator : AbstractValidator<CreatePolygonDto> {
    public CreatePolygonDtoValidator() {
        RuleFor(x => x.Coordinates)
            .NotEmpty()
            .Must(coords => coords.Count >= 3)
            .WithMessage("Polygon must have at least 3 coordinates");

        RuleForEach(x => x.Coordinates)
            .Must(c => c.Latitude >= -90 && c.Latitude <= 90)
            .WithMessage("Latitude must be between -90 and 90");
    }
}

// Registration
services.AddValidatorsFromAssemblyContaining<CreatePolygonDtoValidator>();
services.AddFluentValidationAutoValidation();

// Automatic validation in pipeline
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePolygonDto dto) {
    // Validation happens automatically before this executes
    var result = await _service.CreateAsync(dto);
    return Ok(result);
}
```

**Pros:**
- ✅ **Reusable**: Validators can be used in multiple places
- ✅ **Testable**: Can unit test validators separately
- ✅ **Declarative**: Rules are clear and centralized
- ✅ **Rich features**: Built-in rules for common validations
- ✅ **Automatic integration**: Can auto-validate in pipeline

**Cons:**
- ❌ **Extra dependency**: Another NuGet package
- ❌ **Learning curve**: Need to learn FluentValidation API
- ❌ **Overkill for simple validation**: Our rules are straightforward
- ❌ **Magic behavior**: Auto-validation can be hard to debug
- ❌ **Complexity for GeoJSON**: Polygon closure logic doesn't fit declarative model

**When FluentValidation Shines:**
```csharp
// Complex, reusable validation rules
public class UserRegistrationValidator : AbstractValidator<UserDto> {
    public UserRegistrationValidator(IUserRepository repo) {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) => !await repo.EmailExistsAsync(email))
            .WithMessage("Email already registered");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase")
            .Matches(@"[0-9]").WithMessage("Must contain digit");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120);
    }
}
```

For complex forms with many fields and rules, FluentValidation is worth it.

---

#### Option B: Data Annotations
```csharp
public class CreatePolygonDto {
    [Required(ErrorMessage = "Coordinates are required")]
    [MinLength(3, ErrorMessage = "Polygon must have at least 3 coordinates")]
    public List<CoordinateDto> Coordinates { get; set; }
}

public class CoordinateDto {
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }
}

// Controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePolygonDto dto) {
    if (!ModelState.IsValid) {
        return BadRequest(ModelState);
    }
    // ...
}
```

**Pros:**
- ✅ Built into .NET (no extra dependency)
- ✅ Simple, declarative
- ✅ Integrated with model binding
- ✅ Works well for basic validation

**Cons:**
- ❌ **Limited expressiveness**: Can't validate "polygon is closed"
- ❌ **Attributes clutter POCOs**: Mixing concerns
- ❌ **Hard to unit test**: Need full model binding pipeline
- ❌ **Custom attributes needed** for complex rules

**Custom Attribute Example:**
```csharp
public class ClosedPolygonAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        var coords = value as List<CoordinateDto>;
        if (coords != null && !IsClosedPolygon(coords)) {
            return new ValidationResult("Polygon must be closed");
        }
        return ValidationResult.Success;
    }
}

// Usage
[ClosedPolygon]
public List<CoordinateDto> Coordinates { get; set; }
```

Still requires custom code, now split between attribute definition and usage.

---

#### Option C: Service-Level Validation (Our Choice)
```csharp
public class PolygonService : IPolygonService {
    public async Task<PolygonDto> CreateAsync(CreatePolygonDto dto) {
        // Validation right here - visible and explicit
        if (dto.Coordinates == null || dto.Coordinates.Count < 3) {
            throw new ValidationException("Polygon must have at least 3 coordinates");
        }

        foreach (var coord in dto.Coordinates) {
            if (coord.Latitude < -90 || coord.Latitude > 90) {
                throw new ValidationException($"Invalid latitude: {coord.Latitude}");
            }
            if (coord.Longitude < -180 || coord.Longitude > 180) {
                throw new ValidationException($"Invalid longitude: {coord.Longitude}");
            }
        }

        // Business logic: ensure polygon is closed
        var coordinates = EnsureClosedPolygon(dto.Coordinates);

        // Delegate to repository
        var polygon = await _repository.CreateAsync(coordinates);
        return MapToDto(polygon);
    }

    private List<Coordinate> EnsureClosedPolygon(List<CoordinateDto> coords) {
        var result = coords.Select(c => new Coordinate {
            Latitude = c.Latitude,
            Longitude = c.Longitude
        }).ToList();

        // Auto-close if needed
        if (!result.First().Equals(result.Last())) {
            result.Add(result.First());
        }

        return result;
    }
}
```

**Pros:**
- ✅ **Simple**: No extra dependencies or frameworks
- ✅ **Visible**: Validation logic is right where it's used
- ✅ **Flexible**: Easy to add complex business rules
- ✅ **Testable**: Can unit test service with different inputs
- ✅ **Clear**: No magic, no attributes, just code

**Cons:**
- ⚠️ Validation in service (not separate concern)
- ⚠️ More verbose than declarative approaches

**Why We Chose It:**
1. **YAGNI (You Aren't Gonna Need It)**: Simple validation doesn't need a framework
2. **Visibility**: Developers reading the service see validation immediately
3. **Flexibility**: Business rules (auto-close) fit naturally
4. **No magic**: Explicit is better than implicit for a demo project

**When to Use Each:**

| Scenario | Best Choice |
|----------|-------------|
| 2 entities, simple rules | **Service validation** |
| 10+ entities, complex rules | **FluentValidation** |
| Basic input validation only | **Data Annotations** |
| Mix of validation + business rules | **Service validation** |

---

### Polygon Auto-Closure: A Business Rule

**The Requirement:** "Clicking the starting point again closes the polygon"

This is a **business rule**, not just validation:

```csharp
// Not just validation...
if (!IsClosedPolygon(coordinates)) {
    throw new ValidationException("Polygon must be closed");
}

// ...but a business rule that ensures data integrity
if (!IsClosedPolygon(coordinates)) {
    coordinates.Add(coordinates.First()); // Auto-fix
}
```

**Rationale:**
- GeoJSON spec **requires** closed polygons
- Client **should** close it, but we're defensive
- Server ensures data integrity
- Prevents invalid data in database

**Where This Logic Belongs:**
- ❌ **Not in Controller**: Too early, HTTP concern
- ❌ **Not in Repository**: Too late, should receive valid domain model
- ✅ **In Service**: Perfect - transforms DTO to valid domain entity

---

## 6. Error Handling Approach

### The Decision: RFC 7807 Problem Details

### What is RFC 7807?

**Problem Details for HTTP APIs** (RFC 7807) is a standard format for error responses:

```json
{
  "type": "https://api.example.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Polygon must have at least 3 coordinates",
  "instance": "/api/polygons",
  "traceId": "0HMVFE42N0O8C:00000001"
}
```

### Options Explored

#### Option A: Simple String Messages
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePolygonDto dto) {
    try {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    } catch (ValidationException ex) {
        return BadRequest(ex.Message); // Just a string
    }
}

// Response
HTTP 400 Bad Request
"Polygon must have at least 3 coordinates"
```

**Pros:**
- ✅ Simple to implement
- ✅ Human-readable
- ✅ No extra configuration

**Cons:**
- ❌ **Hard to parse**: Clients must parse strings
- ❌ **No structure**: Can't extract error type programmatically
- ❌ **No standardization**: Each API does it differently
- ❌ **Missing context**: No request ID for debugging

**Client Parsing Problem:**
```javascript
// Client has to do string matching - fragile!
try {
  await createPolygon(data);
} catch (error) {
  if (error.message.includes("coordinates")) {
    highlightCoordinateField();
  } else if (error.message.includes("closed")) {
    showPolygonClosureError();
  }
  // Brittle - breaks if error messages change
}
```

---

#### Option B: Custom Error Object
```csharp
public class ApiError {
    public string Code { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
}

// Usage
catch (ValidationException ex) {
    return BadRequest(new ApiError {
        Code = "VALIDATION_ERROR",
        Message = ex.Message,
        Errors = new Dictionary<string, string[]> {
            { "coordinates", new[] { "Must have at least 3 coordinates" } }
        }
    });
}

// Response
{
  "code": "VALIDATION_ERROR",
  "message": "Polygon must have at least 3 coordinates",
  "errors": {
    "coordinates": ["Must have at least 3 coordinates"]
  }
}
```

**Pros:**
- ✅ Structured, parseable
- ✅ Can include field-specific errors
- ✅ Client can handle programmatically

**Cons:**
- ❌ **Not a standard**: Custom format
- ❌ **Reinventing the wheel**: Problem Details already exists
- ❌ **Inconsistent across APIs**: Every team creates different formats
- ❌ **Missing features**: No traceId, type URI, etc.

---

#### Option C: RFC 7807 Problem Details (Our Choice)
```csharp
// ASP.NET Core has built-in support
public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Enable Problem Details
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Global exception handler
        app.UseExceptionHandler();

        app.Run();
    }
}

// Custom exception handler
app.UseExceptionHandler(errorApp => {
    errorApp.Run(async context => {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var problemDetails = exception switch {
            ValidationException vex => new ProblemDetails {
                Type = "https://api.example.com/errors/validation",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = vex.Message,
                Instance = context.Request.Path
            },
            NotFoundException nex => new ProblemDetails {
                Type = "https://api.example.com/errors/not-found",
                Title = "Resource Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = nex.Message,
                Instance = context.Request.Path
            },
            _ => new ProblemDetails {
                Type = "https://api.example.com/errors/server-error",
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred",
                Instance = context.Request.Path
            }
        };

        // Add trace ID for debugging
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = problemDetails.Status ?? 500;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Response
{
  "type": "https://api.example.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Polygon must have at least 3 coordinates",
  "instance": "/api/polygons",
  "traceId": "0HMVFE42N0O8C:00000001"
}
```

**Pros:**
- ✅ **Industry standard**: RFC 7807, widely adopted
- ✅ **Built into ASP.NET Core**: Minimal code
- ✅ **Structured**: Consistent format across all endpoints
- ✅ **Extensible**: Can add custom properties
- ✅ **Debugging friendly**: Includes traceId for correlation
- ✅ **Client-friendly**: Standard parsing logic

**Cons:**
- ⚠️ Slightly more verbose than simple strings (but worth it)

**Client Benefits:**
```typescript
// TypeScript client with type safety
interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  traceId?: string;
}

try {
  await createPolygon(data);
} catch (error) {
  const problem: ProblemDetails = error.response.data;

  // Type-safe handling
  switch (problem.type) {
    case 'https://api.example.com/errors/validation':
      showValidationError(problem.detail);
      break;
    case 'https://api.example.com/errors/not-found':
      showNotFoundError();
      break;
    default:
      logError(problem.traceId); // Send to support
      showGenericError();
  }
}
```

**Why We Chose It:**
1. **Professional standard**: Shows knowledge of HTTP APIs best practices
2. **Built-in support**: ASP.NET Core makes it easy
3. **Better debugging**: TraceId helps correlate errors across logs
4. **Client experience**: Structured errors are easier to handle

---

### Exception Hierarchy

We define custom exceptions for different error types:

```csharp
// Base exception
public class DomainException : Exception {
    public DomainException(string message) : base(message) { }
}

// Validation errors (400)
public class ValidationException : DomainException {
    public ValidationException(string message) : base(message) { }
}

// Not found errors (404)
public class NotFoundException : DomainException {
    public NotFoundException(string message) : base(message) { }
}

// Usage in service
public async Task<PolygonDto> GetByIdAsync(string id) {
    var polygon = await _repository.GetByIdAsync(id);
    if (polygon == null) {
        throw new NotFoundException($"Polygon with ID '{id}' not found");
    }
    return MapToDto(polygon);
}
```

**Benefits:**
- Type-safe exception handling
- Clear intent (validation vs not-found vs server error)
- Easy to map to HTTP status codes
- Follows exception hierarchy best practices

---

## 7. Repository Pattern Design

### The Decision: Explicit Interfaces vs Generic Repository

### Options Explored

#### Option A: Generic Repository
```csharp
public interface IRepository<T> where T : class {
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
    Task DeleteAllAsync();
}

// Single implementation for all entities
public class MongoRepository<T> : IRepository<T> where T : class {
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoDatabase database, string collectionName) {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T> GetByIdAsync(string id) {
        return await _collection.Find(FilterById(id)).FirstOrDefaultAsync();
    }
    // ... other methods
}

// Registration
services.AddScoped<IRepository<Polygon>>(sp =>
    new MongoRepository<Polygon>(database, "polygons"));
services.AddScoped<IRepository<MapObject>>(sp =>
    new MongoRepository<MapObject>(database, "objects"));
```

**Pros:**
- ✅ **DRY principle**: Write once, use for all entities
- ✅ **Less code**: Single implementation
- ✅ **Consistent API**: All repositories work the same

**Cons:**
- ❌ **Lowest common denominator**: Can't add entity-specific methods
- ❌ **Leaky abstraction**: What if polygons need geo queries?
- ❌ **Forces compromise**: All entities must fit same interface
- ❌ **Complex specialized queries**: Have to break abstraction anyway

**Real-World Problem:**
```csharp
// How do you add this polygon-specific query?
Task<List<Polygon>> FindPolygonsContainingPointAsync(double lat, double lon);

// Can't add to IRepository<T> - not all entities have geometry!
// Have to cast or create IPolygonRepository anyway:
var polygonRepo = (IRepository<Polygon>)_repository; // ❌ Ugly
var specialized = _repository as IPolygonRepository; // ❌ Defeats generic purpose
```

---

#### Option B: Explicit Interface Per Entity (Our Choice)
```csharp
// Polygon Repository
public interface IPolygonRepository {
    Task<Polygon> GetByIdAsync(string id);
    Task<List<Polygon>> GetAllAsync();
    Task<Polygon> CreateAsync(List<Coordinate> coordinates);
    Task DeleteAsync(string id);
    Task DeleteAllAsync();

    // Polygon-specific geo query
    Task<List<Polygon>> FindPolygonsContainingPointAsync(double latitude, double longitude);
}

// Object Repository
public interface IMapObjectRepository {
    Task<MapObject> GetByIdAsync(string id);
    Task<List<MapObject>> GetAllAsync();
    Task<MapObject> CreateAsync(Coordinate location, string objectType);
    Task<List<MapObject>> CreateBatchAsync(List<(Coordinate location, string type)> objects);
    Task DeleteAsync(string id);
    Task DeleteAllAsync();

    // Object-specific query
    Task<List<MapObject>> GetByTypeAsync(string objectType);
    Task<List<MapObject>> GetObjectsNearPointAsync(double latitude, double longitude, double radiusKm);
}
```

**Pros:**
- ✅ **Explicit intent**: Method names match domain concepts
- ✅ **Entity-specific operations**: Each can have unique methods
- ✅ **Type safety**: Parameters match entity needs
- ✅ **No compromises**: Interface designed for exact use case
- ✅ **Easy to extend**: Add methods without affecting other entities
- ✅ **Clear contracts**: Developers see exactly what's available

**Cons:**
- ⚠️ More code (but only 2 interfaces for 2 entities)
- ⚠️ Some duplication (GetByIdAsync appears twice)

**Why We Chose It:**
1. **Small project**: 2 entities don't justify generic abstraction
2. **Different needs**: Polygons and Objects have different query requirements
3. **Clarity**: `IPolygonRepository.CreateAsync(coordinates)` is clearer than `IRepository<Polygon>.CreateAsync(polygon)`
4. **Future extensibility**: Easy to add geo queries later

**Rule of Thumb:**
- **2-3 entities**: Explicit interfaces
- **10+ similar entities**: Consider generic repository
- **Mixed needs**: Explicit interfaces even if more entities

---

#### Option C: Base Interface + Specialized Interfaces
```csharp
// Base interface
public interface IRepository<T> where T : class {
    Task<T> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task DeleteAsync(string id);
    Task DeleteAllAsync();
}

// Specialized interfaces extend base
public interface IPolygonRepository : IRepository<Polygon> {
    Task<Polygon> CreateAsync(List<Coordinate> coordinates);
    Task<List<Polygon>> FindPolygonsContainingPointAsync(double lat, double lon);
}

public interface IMapObjectRepository : IRepository<MapObject> {
    Task<MapObject> CreateAsync(Coordinate location, string objectType);
    Task<List<MapObject>> GetByTypeAsync(string objectType);
}
```

**Pros:**
- ✅ Reduces duplication of common methods
- ✅ Allows entity-specific methods

**Cons:**
- ❌ **Over-engineering**: For 2 entities, this is excessive
- ❌ **Generic constraints**: T must satisfy base interface constraints
- ❌ **Complexity**: Two layers of abstraction

**When This Makes Sense:**
Large systems with 20+ entities where 80% share common operations but 20% need specialization.

---

## 8. Database Design

### MongoDB Collection Design

We use **two collections** with **GeoJSON geometry**:

```javascript
// Polygons collection
{
  "_id": ObjectId("..."),
  "geometry": {
    "type": "Polygon",
    "coordinates": [[
      [-74.0060, 40.7128],
      [-73.9855, 40.7580],
      [-74.0100, 40.7600],
      [-74.0060, 40.7128]  // Closed: first == last
    ]]
  },
  "createdAt": ISODate("2026-01-07T10:30:00Z")
}

// Objects collection
{
  "_id": ObjectId("..."),
  "location": {
    "type": "Point",
    "coordinates": [-74.0060, 40.7128]  // [lon, lat]
  },
  "objectType": "Jeep",
  "createdAt": ISODate("2026-01-07T10:35:00Z")
}
```

### Index Strategy

```javascript
// Polygons: 2dsphere index for geospatial queries
db.polygons.createIndex({ geometry: "2dsphere" });

// Objects: 2dsphere index + type index
db.objects.createIndex({ location: "2dsphere" });
db.objects.createIndex({ objectType: 1 }); // For filtering by type

// Both: createdAt for sorting recent items
db.polygons.createIndex({ createdAt: -1 });
db.objects.createIndex({ createdAt: -1 });
```

**Why These Indexes:**

1. **2dsphere (geospatial)**
   - Enables `$geoIntersects`, `$geoWithin`, `$near` queries
   - Required for polygon containment checks
   - Supports spherical geometry (Earth's curvature)

2. **objectType**
   - Fast filtering: "Show me all Jeeps"
   - Small cardinality (few unique types)
   - Good candidate for index

3. **createdAt**
   - Supports "recent items" queries
   - Enables pagination (skip/take)
   - Useful for time-based analytics

**Index Trade-offs:**
- ✅ Faster queries
- ❌ Slower writes (index must be updated)
- ❌ Storage overhead (~10-20% per index)

For read-heavy map apps, indexes are worth it.

---

### Alternative Schema Designs Considered

#### Option A: Embedded Documents (Denormalization)
```javascript
// Single "map" collection with embedded arrays
{
  "_id": ObjectId("..."),
  "mapName": "New York City",
  "polygons": [
    { "geometry": {...}, "createdAt": "..." },
    { "geometry": {...}, "createdAt": "..." }
  ],
  "objects": [
    { "location": {...}, "objectType": "Marker", "createdAt": "..." },
    { "location": {...}, "objectType": "Jeep", "createdAt": "..." }
  ]
}
```

**Pros:**
- ✅ Single document retrieval
- ✅ Atomic updates (update entire map at once)

**Cons:**
- ❌ **Document size limits**: MongoDB has 16MB limit
- ❌ **Inefficient updates**: Updating one polygon fetches/updates entire map
- ❌ **No individual polygon IDs**: Hard to delete specific polygon
- ❌ **Doesn't match assignment**: Assignment treats them as separate resources

---

#### Option B: Separate Collections (Our Choice)
```javascript
// Polygons collection
{ "_id": ObjectId("..."), "geometry": {...} }

// Objects collection
{ "_id": ObjectId("..."), "location": {...}, "objectType": "..." }
```

**Pros:**
- ✅ **Matches assignment**: Separate API endpoints = separate collections
- ✅ **Scalable**: No document size limits
- ✅ **Efficient**: Update only what changed
- ✅ **Independent indexes**: Optimize each collection separately
- ✅ **Clear separation**: Different entities, different collections

**Cons:**
- ⚠️ Requires 2 requests to load full map (but client needs this anyway for UI structure)

---

## Summary: Key Takeaways

### Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Architecture Pattern** | Clean Architecture | Separation of concerns, testability, professionalism |
| **Batch Endpoint** | Separate `/batch` | Matches assignment, explicit contracts |
| **Delete All** | Add to both resources | API consistency, better UX |
| **Update Operations** | Omit (not in assignment) | Follow requirements, avoid gold-plating |
| **Data Model** | DTO + Domain + Document | Clean boundaries, database independence |
| **Coordinate Order** | [Lat, Lon] in API | Natural, matches client library |
| **Validation** | Service-level | Simple, visible, flexible (no framework needed) |
| **Error Handling** | RFC 7807 Problem Details | Industry standard, structured errors |
| **Repository** | Explicit interfaces | Entity-specific needs, clarity |
| **Database** | Separate collections | Matches API design, scalable |

### Design Principles Applied

1. **YAGNI (You Aren't Gonna Need It)**
   - No generic repositories for 2 entities
   - No FluentValidation for simple rules
   - No UPDATE endpoints (not in requirements)

2. **Separation of Concerns**
   - Controllers: HTTP only
   - Services: Business logic
   - Repositories: Data access

3. **Explicit Over Implicit**
   - Named Coordinate properties vs arrays
   - Specific repository interfaces vs generic
   - Visible validation vs hidden attributes

4. **Professional Standards**
   - Clean Architecture
   - RFC 7807 errors
   - Async/await throughout
   - Dependency injection

### Interview Talking Points

When discussing this project:

1. **Show decision-making process**
   - "I considered generic repositories but chose explicit interfaces because..."
   - "The assignment didn't specify UPDATE, so I focused on what was asked..."

2. **Demonstrate awareness of trade-offs**
   - "Service-level validation is simpler here, but FluentValidation would make sense if..."
   - "I added the batch endpoint to match the assignment's 'list of objects' requirement..."

3. **Highlight extensibility**
   - "The architecture makes it easy to add UPDATE operations later..."
   - "Separate collections enable future geo queries like 'find polygons near point'..."

4. **Connect to real-world experience**
   - "In production, I'd add authentication, rate limiting, and caching..."
   - "The DTO pattern prevents MongoDB types from leaking into the API contract..."

---

## Conclusion

Building a well-architected API requires balancing:
- **Simplicity** vs **Over-engineering**
- **Following requirements** vs **Improving design**
- **Quick implementation** vs **Long-term maintainability**

This project demonstrates professional patterns while staying focused on the assignment scope. Each decision prioritizes clarity, testability, and real-world best practices.

**Most Important Lesson:**
> Architecture is about **making trade-offs visible and intentional**, not about using the most advanced patterns.

Good architecture serves the requirements, the team, and the users - in that order.
