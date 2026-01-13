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

**ASP.NET Core** is Microsoft's cross-platform, high-performance framework for building modern web applications and APIs in C#. It's a complete rewrite of the older ASP.NET Framework, designed from the ground up to be fast, modular, and cloud-ready.

### The Big Picture: Where ASP.NET Core Fits

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Your Application                              │
│                    (MapServer - your code)                          │
├─────────────────────────────────────────────────────────────────────┤
│                       ASP.NET Core                                   │
│   (Web framework - routing, controllers, JSON, DI, middleware)      │
├─────────────────────────────────────────────────────────────────────┤
│                         .NET Runtime                                 │
│        (C# compiler, garbage collection, base libraries)            │
├─────────────────────────────────────────────────────────────────────┤
│                    Operating System                                  │
│              (Windows, Linux, macOS, Docker)                        │
└─────────────────────────────────────────────────────────────────────┘
```

### What Problems Does It Solve?

Without a framework, building a web API would require you to:
1. Create a TCP socket and listen for connections
2. Parse raw HTTP text (`GET /api/polygons HTTP/1.1\r\nHost: localhost...`)
3. Match URL patterns to your code manually
4. Convert JSON strings to C# objects by hand
5. Handle errors, security, logging, configuration...

**ASP.NET Core does ALL of this for you**, letting you focus on business logic.

### The Request Pipeline (Middleware)

When a request arrives, it flows through a **pipeline** of middleware components:

```
HTTP Request
     ↓
┌─────────────────┐
│  Exception      │  ← Catches errors, returns 500 responses
│  Handler        │
└────────┬────────┘
         ↓
┌─────────────────┐
│  CORS           │  ← Adds cross-origin headers
│  Middleware     │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Routing        │  ← Matches URL to controller/action
│  Middleware     │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Your           │  ← Your controller method runs here
│  Controller     │
└────────┬────────┘
         ↓
HTTP Response
```

Each middleware can:
- **Process the request** before passing it along
- **Short-circuit** the pipeline (return early without calling the next middleware)
- **Process the response** on the way back out

This is configured in `Program.cs`:
```csharp
var app = builder.Build();

// Each Use/Map adds middleware to the pipeline
app.UseExceptionHandler("/error");  // First in, last out
app.UseCors("AllowReactApp");       // CORS headers
app.MapControllers();                // Route to controllers
```

### Key Components Provided by ASP.NET Core

| Component | What It Does | Without It You'd Have To... |
|-----------|--------------|----------------------------|
| **Kestrel** | High-performance web server | Write socket code, handle HTTP parsing |
| **Routing** | Maps URLs to controller methods | Write string matching logic, extract parameters |
| **Model Binding** | Converts JSON/form data to C# objects | Parse JSON manually, validate types |
| **Action Results** | Creates HTTP responses | Build response strings, set headers |
| **DI Container** | Manages object lifetimes and dependencies | Create factories, manage singletons manually |
| **Configuration** | Reads appsettings.json, environment vars | Write file parsing, handle overrides |
| **Logging** | Structured logging with multiple outputs | Write logging infrastructure |

### Program.cs: The Entry Point

Every ASP.NET Core app starts with `Program.cs`. Let's break down what's actually happening:

```csharp
// PHASE 1: BUILDING - Configure all services BEFORE the app runs
var builder = WebApplication.CreateBuilder(args);

// Register services with the DI container
// These are CONFIGURATIONS - nothing is created yet
builder.Services.AddControllers();           // "I'll need controller support"
builder.Services.AddScoped<IPolygonService, PolygonService>();  // "When someone asks for IPolygonService, give them PolygonService"

// PHASE 2: BUILD - Lock in configuration, create the app
var app = builder.Build();  // After this, you can't add more services

// PHASE 3: CONFIGURE PIPELINE - Set up middleware order
app.UseCors("AllowReactApp");  // Middleware 1
app.MapControllers();          // Middleware 2 (terminal - handles the request)

// PHASE 4: RUN - Start listening for requests
app.Run();  // Blocks here, processing requests until shutdown
```

**Why two phases (builder vs app)?**
- During building: You're describing what you WANT
- After Build(): The container is sealed, optimized, and ready
- This separation enables performance optimizations and catches configuration errors early

### How Requests Are Handled (The Full Journey)

```
1. HTTP Request arrives: POST /api/polygons
   Content-Type: application/json
   Body: {"coordinates": [...]}

2. Kestrel (web server) receives raw bytes, parses HTTP

3. Routing middleware examines the URL and method:
   - Finds [Route("api/[controller]")] on PolygonsController
   - Finds [HttpPost] on Create method
   - Match! Route to PolygonsController.Create()

4. Model Binding reads the request body:
   - Sees Content-Type: application/json
   - Deserializes JSON to CreatePolygonRequest object
   - Validates [Required] attributes, data types

5. DI Container creates the controller:
   - Controller needs IPolygonService? Let me create that
   - PolygonService needs IPolygonRepository? Creating that too
   - Repository needs MongoDbContext? Here you go
   - (All dependencies resolved automatically!)

6. Your controller method executes:
   - Your code runs with all dependencies injected
   - Returns CreatedAtAction(...)

7. Action Result Executor:
   - Serializes PolygonDto to JSON
   - Sets status code 201
   - Adds Location header

8. Response flows back through middleware (reverse order)

9. Kestrel sends HTTP response bytes to client
```

### ASP.NET Core vs. Other Frameworks

| Feature | ASP.NET Core | Node.js/Express | Python/Flask |
|---------|--------------|-----------------|--------------|
| Language | C# (compiled, strongly typed) | JavaScript (interpreted) | Python (interpreted) |
| Performance | Very fast (compiled, optimized) | Good | Moderate |
| Type Safety | Compile-time errors | Runtime errors | Runtime errors |
| Built-in DI | Yes | No (need library) | No (need library) |
| Tooling | Excellent (Visual Studio, Rider) | Good | Good |

### Why ASP.NET Core for This Project?

1. **Strong typing**: Catch errors at compile time, not runtime
2. **Performance**: Handles thousands of requests per second
3. **MongoDB driver**: Excellent official C# driver
4. **GeoJSON support**: Native MongoDB GeoJSON types in C#
5. **Career relevance**: Widely used in enterprise applications

---

## Namespaces

```csharp
namespace MapServer.DTOs;
```

**Simple Analogy**: Namespaces are like folders on your computer, combined with a "last name" system. Just as there can be "John Smith from Accounting" and "John Smith from Marketing" (same name, different departments), you can have classes with the same name in different namespaces.

### The Problem Namespaces Solve

Imagine a large project with multiple developers. Developer A creates a `Logger` class. Developer B also creates a `Logger` class with different functionality. Without namespaces, these would conflict - the compiler wouldn't know which `Logger` you mean.

```csharp
// Without namespaces - CONFLICT! Which Logger?
class Logger { }  // Developer A's
class Logger { }  // Developer B's - ERROR: already defined!

// With namespaces - NO CONFLICT
namespace CompanyA.Utilities {
    class Logger { }  // CompanyA.Utilities.Logger
}
namespace CompanyB.Utilities {
    class Logger { }  // CompanyB.Utilities.Logger - Different full name!
}
```

### Namespace Syntax: Two Styles

**Traditional (block) syntax** - wraps code in braces:
```csharp
namespace MapServer.DTOs
{
    public class PolygonDto
    {
        // Everything inside is in this namespace
    }

    public class Coordinate
    {
        // Also in MapServer.DTOs
    }
}
```

**File-scoped syntax** (C# 10+) - cleaner, no extra indentation:
```csharp
namespace MapServer.DTOs;  // Note the semicolon!

public class PolygonDto
{
    // In MapServer.DTOs namespace
}

public class Coordinate
{
    // Also in MapServer.DTOs (entire file is in this namespace)
}
```

This project uses **file-scoped namespaces** for cleaner code.

### The `using` Directive: Importing Namespaces

Without `using`, you'd need the full name every time:
```csharp
// Tedious - full names everywhere
MapServer.DTOs.PolygonDto polygon = new MapServer.DTOs.PolygonDto();
System.Collections.Generic.List<MapServer.DTOs.Coordinate> coords;
```

With `using`, you import the namespace once:
```csharp
using MapServer.DTOs;                    // Import our DTOs
using System.Collections.Generic;        // Import List, Dictionary, etc.

// Now we can use short names
PolygonDto polygon = new PolygonDto();
List<Coordinate> coords;
```

### Global Usings (C# 10+)

Some namespaces are needed in almost every file. Instead of adding `using` to each file, define them once globally:

```csharp
// In GlobalUsings.cs or your .csproj file
global using System;
global using System.Collections.Generic;
global using System.Threading.Tasks;
```

Now these are automatically available in ALL files in the project.

### Implicit Usings

ASP.NET Core projects enable `<ImplicitUsings>enable</ImplicitUsings>` in the .csproj file, which automatically includes common namespaces like `System`, `System.Collections.Generic`, `System.Linq`, etc.

### Namespace Conventions in This Project

```
MapServer/
├── Controllers/     → namespace MapServer.Controllers
├── Services/        → namespace MapServer.Services
├── Repositories/    → namespace MapServer.Repositories
├── Models/          → namespace MapServer.Models
├── DTOs/            → namespace MapServer.DTOs
└── Data/            → namespace MapServer.Data
```

**The rule**: Namespace = Project name + Folder path

This convention means:
1. You can find a class by its namespace (just follow the folders)
2. The code is organized logically
3. Related classes are grouped together

### Resolving Namespace Conflicts

Sometimes you need two classes with the same name from different namespaces:

```csharp
using MapServer.DTOs;
using MapServer.Models;

// PROBLEM: Both namespaces have "Polygon" - which one?
Polygon p;  // Ambiguous!

// SOLUTION 1: Use full names
MapServer.DTOs.PolygonDto dto;
MapServer.Models.Polygon model;

// SOLUTION 2: Alias one namespace
using DtoModels = MapServer.DTOs;

DtoModels.PolygonDto dto;
Polygon model;  // Uses MapServer.Models.Polygon

// SOLUTION 3: Alias the type directly
using PolygonModel = MapServer.Models.Polygon;

PolygonDto dto;
PolygonModel model;
```

### Nested Namespaces

Namespaces can be nested for finer organization:

```csharp
namespace MapServer.Services.Validation
{
    public class CoordinateValidator { }
}

// Usage:
using MapServer.Services.Validation;
// or
var validator = new MapServer.Services.Validation.CoordinateValidator();
```

### Why This Matters for Understanding Code

When you see this at the top of a file:
```csharp
using MapServer.DTOs;
using MapServer.Repositories;
using MongoDB.Driver;
```

You immediately know:
- This file uses DTOs (data transfer objects) from our project
- This file interacts with repositories (data access layer)
- This file uses MongoDB directly (probably a repository implementation)

**Namespaces are documentation** - they tell you what categories of code are involved.

---

## Classes and Objects

```csharp
public class Coordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

**Simple Analogy**: A class is like a blueprint for a house. It specifies "a house has 3 bedrooms, 2 bathrooms, a kitchen." An object is an actual house built from that blueprint - you can build many houses from the same blueprint, each is its own independent object.

### Classes: The Template

A class defines:
1. **What data it holds** (properties/fields) - "A Coordinate has a Latitude and Longitude"
2. **What it can do** (methods) - "A Coordinate can calculate distance to another Coordinate"
3. **How it's created** (constructors) - "To create a Coordinate, you must provide latitude and longitude"

```csharp
public class Coordinate
{
    // DATA: What a Coordinate "has"
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // BEHAVIOR: What a Coordinate "does"
    public double DistanceTo(Coordinate other)
    {
        // Calculate distance using Haversine formula
        // ... implementation
    }

    // CREATION: How to build a Coordinate
    public Coordinate(double lat, double lon)
    {
        Latitude = lat;
        Longitude = lon;
    }
}
```

### Objects: The Instances

An object is a **specific instance** of a class, with actual values:

```csharp
// Class = Blueprint, Object = Actual thing
// "Coordinate" = the concept, "telAviv" = a specific coordinate

Coordinate telAviv = new Coordinate(32.0853, 34.7818);
Coordinate jerusalem = new Coordinate(31.7683, 35.2137);

// Each object has its own data
Console.WriteLine(telAviv.Latitude);    // 32.0853
Console.WriteLine(jerusalem.Latitude);  // 31.7683

// Objects can interact with each other
double distance = telAviv.DistanceTo(jerusalem);
```

### The `new` Keyword: Creating Objects

`new` does three things:
1. **Allocates memory** for the object
2. **Calls the constructor** to initialize it
3. **Returns a reference** to the new object

```csharp
// Three ways to create objects:

// 1. Constructor with parameters
var coord1 = new Coordinate(32.5, 35.2);

// 2. Object initializer syntax (if parameterless constructor exists)
var coord2 = new Coordinate { Latitude = 32.5, Longitude = 35.2 };

// 3. Target-typed new (C# 9+) - compiler knows the type
Coordinate coord3 = new(32.5, 35.2);
```

### Access Modifiers: Who Can See What

```csharp
public class BankAccount
{
    public string AccountNumber { get; set; }     // Anyone can see
    private decimal _balance;                      // Only this class can see
    protected string _internalNotes;              // This class + children
    internal void ProcessPayment() { }            // Only this assembly (project)

    public decimal GetBalance()
    {
        // public method that exposes private data safely
        return _balance;
    }
}
```

| Modifier | Accessible From |
|----------|-----------------|
| `public` | Anywhere |
| `private` | Only inside this class |
| `protected` | This class and classes that inherit from it |
| `internal` | Anywhere in the same project/assembly |
| `protected internal` | Same assembly OR inheriting classes |
| `private protected` | Inheriting classes in the same assembly only |

**Default access**: If you don't specify, classes are `internal` and members are `private`.

### Why `private` Matters: Encapsulation

Encapsulation = hiding implementation details, exposing only what's needed.

```csharp
// BAD: Everything public - anyone can break your class
public class BankAccount
{
    public decimal Balance;  // Anyone can set this to -1000000!
}

// GOOD: Private data, controlled access
public class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;  // Read-only public access

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds");
        _balance -= amount;
    }
}
```

Benefits:
- **Validation**: You control how data changes
- **Flexibility**: You can change internal implementation without breaking users
- **Safety**: Users can't put your object in an invalid state

### Constructors: Object Initialization

A constructor is a special method that runs when creating an object:

```csharp
public class Polygon
{
    public string Id { get; set; }
    public List<Coordinate> Coordinates { get; set; }
    private readonly DateTime _createdAt;

    // Default constructor (parameterless)
    public Polygon()
    {
        Id = Guid.NewGuid().ToString();
        Coordinates = new List<Coordinate>();
        _createdAt = DateTime.UtcNow;
    }

    // Parameterized constructor
    public Polygon(List<Coordinate> coordinates)
    {
        Id = Guid.NewGuid().ToString();
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
        _createdAt = DateTime.UtcNow;
    }

    // Constructor that calls another constructor
    public Polygon(string id, List<Coordinate> coordinates) : this(coordinates)
    {
        Id = id;  // Override the generated ID
    }
}
```

### Static vs Instance Members

**Instance members** belong to each object separately:
```csharp
var coord1 = new Coordinate { Latitude = 32 };
var coord2 = new Coordinate { Latitude = 35 };
// coord1.Latitude and coord2.Latitude are different values
```

**Static members** belong to the class itself, shared by all objects:
```csharp
public class Coordinate
{
    // Instance - each Coordinate has its own
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Static - ONE shared value for the class
    public static int TotalCoordinatesCreated { get; private set; }

    public Coordinate()
    {
        TotalCoordinatesCreated++;  // Increments the shared counter
    }

    // Static method - doesn't need an object to call
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}

// Usage:
var c1 = new Coordinate();
var c2 = new Coordinate();
Console.WriteLine(Coordinate.TotalCoordinatesCreated);  // 2

double radians = Coordinate.DegreesToRadians(45);  // Called on CLASS, not object
```

### The `this` Keyword

`this` refers to the current object instance:

```csharp
public class Coordinate
{
    private double _latitude;

    public Coordinate(double latitude)
    {
        // Without 'this', it would be: latitude = latitude (useless!)
        this._latitude = latitude;
    }

    public bool IsSameLocation(Coordinate other)
    {
        // 'this' is the current object
        return this._latitude == other._latitude;
    }
}
```

### Inheritance: Classes Building on Classes

A class can inherit from another, gaining its properties and methods:

```csharp
// Base class
public class MapObject
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Derived class - inherits everything from MapObject
public class Marker : MapObject
{
    public double Latitude { get; set; }    // Marker-specific
    public double Longitude { get; set; }   // Marker-specific
    public string IconUrl { get; set; }     // Marker-specific
    // Also has Id, Name, CreatedAt from MapObject!
}

// Usage:
var marker = new Marker
{
    Id = "123",              // From MapObject
    Name = "My Location",    // From MapObject
    Latitude = 32.5,         // From Marker
    Longitude = 35.2         // From Marker
};
```

### Value Types vs Reference Types

**Value types** (structs, int, double, bool) store data directly:
```csharp
int a = 5;
int b = a;  // b gets a COPY of the value
b = 10;     // a is still 5
```

**Reference types** (classes) store a reference (pointer) to the data:
```csharp
Coordinate a = new Coordinate { Latitude = 32 };
Coordinate b = a;  // b points to the SAME object
b.Latitude = 99;   // a.Latitude is also 99 now!
```

This is crucial for understanding how objects behave when passed to methods!

---

## Properties (get/set)

```csharp
public double Latitude { get; set; }
```

**Simple Analogy**: Properties are like controlled doors to your data. A field is an open doorway anyone can walk through. A property is a door with a security guard (getter/setter) that can check IDs, log entries, or deny access.

### Fields vs Properties: Why the Difference?

**Fields** are simple variables - direct access:
```csharp
public class Coordinate
{
    public double latitude;  // FIELD - direct access, no control
}

var c = new Coordinate();
c.latitude = -9999;  // Invalid value, but nothing stops this!
```

**Properties** add a layer of control:
```csharp
public class Coordinate
{
    public double Latitude { get; set; }  // PROPERTY - controlled access
}
```

### How Properties Actually Work (Behind the Scenes)

When you write `{ get; set; }`, the compiler generates this:

```csharp
// What you write:
public double Latitude { get; set; }

// What the compiler generates:
private double _latitude;  // Hidden "backing field"

public double get_Latitude()
{
    return _latitude;
}

public void set_Latitude(double value)
{
    _latitude = value;
}
```

So `{ get; set; }` is just shorthand - called an **auto-property**.

### All Property Variations Explained

```csharp
public class Coordinate
{
    // 1. READ-WRITE: Anyone can read and write
    public double Latitude { get; set; }

    // 2. READ-ONLY (auto-property): Can only be set in constructor
    public string Id { get; }

    // 3. PRIVATE SET: Anyone can read, only this class can write
    public DateTime CreatedAt { get; private set; }

    // 4. INIT-ONLY (C# 9+): Can be set during initialization, then read-only
    public string CreatedBy { get; init; }

    // 5. EXPRESSION-BODIED (computed): No backing field, calculated each time
    public bool IsValid => Latitude >= -90 && Latitude <= 90;

    // 6. FULL PROPERTY: Manual backing field with custom logic
    private double _longitude;
    public double Longitude
    {
        get { return _longitude; }
        set
        {
            if (value < -180 || value > 180)
                throw new ArgumentOutOfRangeException(nameof(value));
            _longitude = value;
        }
    }

    public Coordinate()
    {
        Id = Guid.NewGuid().ToString();  // Can set { get; } in constructor
        CreatedAt = DateTime.UtcNow;
    }
}

// Usage:
var coord = new Coordinate
{
    Latitude = 32.5,
    CreatedBy = "Admin"  // init-only: can set here
};

coord.Latitude = 33.0;      // OK - { get; set; }
// coord.Id = "xyz";        // ERROR - { get; } is read-only
// coord.CreatedBy = "X";   // ERROR - init-only, already initialized
coord.Longitude = 200;      // THROWS - validation in setter
```

### Property Access Modifiers

You can have different access levels for get and set:

```csharp
public class User
{
    // Anyone can read, only this class can write
    public string Name { get; private set; }

    // Anyone can read, this class and children can write
    public int Age { get; protected set; }

    // Same assembly can read, only this class can write
    internal string InternalId { get; private set; }
}
```

### When to Use Each Type

| Property Type | Use When |
|---------------|----------|
| `{ get; set; }` | Mutable data, DTOs, simple data containers |
| `{ get; }` | Immutable after construction (IDs, creation dates) |
| `{ get; private set; }` | Class manages the value, others can read (counters, calculated states) |
| `{ get; init; }` | Immutable objects, but need to set during initialization |
| Expression-bodied `=>` | Computed/derived values that don't need storage |
| Full property | When you need validation, logging, events, or complex logic |

### Full Properties: When You Need More Control

```csharp
public class BankAccount
{
    private decimal _balance;
    private List<string> _transactionLog = new();

    public decimal Balance
    {
        get
        {
            // Could add logging, access control, lazy loading
            return _balance;
        }
        private set
        {
            // Validation
            if (value < 0)
                throw new InvalidOperationException("Balance cannot be negative");

            // Logging
            _transactionLog.Add($"{DateTime.Now}: Balance changed from {_balance} to {value}");

            // Events (if wired up)
            var oldValue = _balance;
            _balance = value;
            // OnBalanceChanged?.Invoke(oldValue, value);
        }
    }
}
```

### Expression-Bodied Properties (Computed Properties)

For simple calculations, use the `=>` syntax:

```csharp
public class Rectangle
{
    public double Width { get; set; }
    public double Height { get; set; }

    // Computed property - calculated fresh each time it's accessed
    public double Area => Width * Height;

    // Same as:
    // public double Area { get { return Width * Height; } }

    // No backing field! Value is computed from Width and Height
}

var rect = new Rectangle { Width = 5, Height = 3 };
Console.WriteLine(rect.Area);  // 15
rect.Width = 10;
Console.WriteLine(rect.Area);  // 30 (recalculated)
```

### The `required` Modifier (C# 11+)

Ensure properties are set during initialization:

```csharp
public class Person
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int Age { get; set; }  // Optional
}

// Must provide Name and Email
var person = new Person
{
    Name = "John",
    Email = "john@example.com"
};

// ERROR: 'Name' is required
// var bad = new Person { Email = "test@test.com" };
```

### Default Values for Properties

```csharp
public class PolygonDto
{
    public string Id { get; set; } = string.Empty;  // Default: empty string
    public List<Coordinate> Coordinates { get; set; } = new();  // Default: empty list
    public bool IsVisible { get; set; } = true;  // Default: true
}

// Now you can create without worrying about nulls:
var polygon = new PolygonDto();
polygon.Coordinates.Add(new Coordinate());  // No NullReferenceException!
```

### Why Frameworks Require Properties

Many .NET frameworks (JSON serialization, Entity Framework, ASP.NET model binding) work with properties, not fields:

```csharp
// JSON Serializer looks for properties
public class ApiResponse
{
    public string Message { get; set; }   // ✓ Serialized
    public string message;                 // ✗ Often ignored by serializers
}

// This is why DTOs always use properties
```

### The `nameof` Operator with Properties

Useful for error messages and data binding:

```csharp
public class Coordinate
{
    private double _latitude;

    public double Latitude
    {
        get => _latitude;
        set
        {
            if (value < -90 || value > 90)
                throw new ArgumentOutOfRangeException(
                    nameof(Latitude),  // "Latitude" - refactor-safe!
                    $"Latitude must be between -90 and 90, was {value}");
            _latitude = value;
        }
    }
}
```

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

**Simple Analogy**: An interface is a contract. If a restaurant says "We implement IRestaurant", they're promising to have a menu, take orders, serve food, and accept payment. HOW they do these things (fast food vs fine dining) is their business - but they MUST do them.

### What Is an Interface?

An interface defines **what** a class can do, without specifying **how**:

```csharp
// The contract - WHAT must be done
public interface IPolygonService
{
    Task<List<PolygonDto>> GetAllAsync();           // Must be able to get all polygons
    Task<PolygonDto?> GetByIdAsync(string id);      // Must be able to get one polygon
    Task<PolygonDto> CreateAsync(CreatePolygonRequest request);  // Must be able to create
    Task<bool> DeleteAsync(string id);              // Must be able to delete
}

// The implementation - HOW it's done
public class PolygonService : IPolygonService
{
    private readonly IPolygonRepository _repository;

    public async Task<List<PolygonDto>> GetAllAsync()
    {
        // Actual implementation with database calls, mapping, etc.
        var polygons = await _repository.GetAllAsync();
        return polygons.Select(MapToDto).ToList();
    }

    public async Task<PolygonDto?> GetByIdAsync(string id) { /* ... */ }
    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request) { /* ... */ }
    public async Task<bool> DeleteAsync(string id) { /* ... */ }
}
```

### The Power of Interfaces: Multiple Implementations

The same interface can have different implementations:

```csharp
// Interface - the contract
public interface IPolygonService
{
    Task<List<PolygonDto>> GetAllAsync();
}

// Implementation 1: Real service talking to MongoDB
public class PolygonService : IPolygonService
{
    public async Task<List<PolygonDto>> GetAllAsync()
    {
        // Connects to real database
        return await _repository.GetAllAsync();
    }
}

// Implementation 2: Mock service for testing
public class MockPolygonService : IPolygonService
{
    public Task<List<PolygonDto>> GetAllAsync()
    {
        // Returns fake data - no database needed
        return Task.FromResult(new List<PolygonDto>
        {
            new() { Id = "1", Coordinates = new() },
            new() { Id = "2", Coordinates = new() }
        });
    }
}

// Implementation 3: Caching decorator
public class CachedPolygonService : IPolygonService
{
    private readonly IPolygonService _inner;
    private List<PolygonDto>? _cache;

    public CachedPolygonService(IPolygonService inner) => _inner = inner;

    public async Task<List<PolygonDto>> GetAllAsync()
    {
        if (_cache == null)
            _cache = await _inner.GetAllAsync();
        return _cache;
    }
}
```

### Why Code Against Interfaces?

```csharp
// BAD: Depends on concrete class
public class PolygonsController
{
    private readonly PolygonService _service;  // Tied to THIS specific implementation

    public PolygonsController()
    {
        _service = new PolygonService();  // Hard to test, hard to change
    }
}

// GOOD: Depends on interface
public class PolygonsController
{
    private readonly IPolygonService _service;  // Any implementation works!

    public PolygonsController(IPolygonService service)  // Injected from outside
    {
        _service = service;
    }
}
```

Now the controller works with:
- `PolygonService` in production
- `MockPolygonService` in unit tests
- `CachedPolygonService` when you need caching
- Any future implementation you create

**The controller doesn't change** - you just swap what you inject.

### Interface Anatomy

```csharp
public interface IPolygonRepository
{
    // Methods - no body, just signatures
    Task<Polygon?> GetByIdAsync(string id);
    Task<List<Polygon>> GetAllAsync();
    Task CreateAsync(Polygon polygon);
    Task<bool> DeleteAsync(string id);

    // Properties - can be get-only, set-only, or both
    string CollectionName { get; }

    // Events (less common)
    event EventHandler<Polygon>? PolygonCreated;

    // Default interface methods (C# 8+) - can have implementation!
    bool Exists(string id) => GetByIdAsync(id).Result != null;
}
```

### Implementing Multiple Interfaces

A class can implement multiple interfaces:

```csharp
public interface ICreatable<T>
{
    Task<T> CreateAsync(T item);
}

public interface IDeletable
{
    Task<bool> DeleteAsync(string id);
}

public interface IReadable<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
}

// Implements all three!
public class PolygonRepository : ICreatable<Polygon>, IDeletable, IReadable<Polygon>
{
    public Task<Polygon> CreateAsync(Polygon item) { /* ... */ }
    public Task<bool> DeleteAsync(string id) { /* ... */ }
    public Task<Polygon?> GetByIdAsync(string id) { /* ... */ }
    public Task<List<Polygon>> GetAllAsync() { /* ... */ }
}
```

### Interface Inheritance

Interfaces can inherit from other interfaces:

```csharp
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task CreateAsync(T item);
    Task<bool> DeleteAsync(string id);
}

// Combines both interfaces
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T>
{
    // Inherits all methods from both interfaces
    // Can add more if needed
}

// Now you can:
// - Inject IReadRepository if you only need to read
// - Inject IWriteRepository if you only need to write
// - Inject IRepository if you need both
```

### The `I` Prefix Convention

In C#, interfaces start with `I` by convention:

| Type | Naming |
|------|--------|
| Interface | `IPolygonService`, `IRepository`, `ILogger` |
| Class | `PolygonService`, `Repository`, `Logger` |

This makes it instantly clear when you're looking at an interface vs a class.

### Abstract Classes vs Interfaces

Both define contracts, but with key differences:

```csharp
// INTERFACE: Pure contract, no implementation
public interface IAnimal
{
    string Name { get; }
    void MakeSound();
}

// ABSTRACT CLASS: Partial implementation allowed
public abstract class Animal
{
    public string Name { get; set; }      // Implemented
    public abstract void MakeSound();      // Must be implemented by child

    public void Sleep()                    // Implemented - inherited by all
    {
        Console.WriteLine($"{Name} is sleeping");
    }
}
```

| Feature | Interface | Abstract Class |
|---------|-----------|----------------|
| Can have implementation | Yes (C# 8+ default methods) | Yes |
| Can have fields | No | Yes |
| Can have constructors | No | Yes |
| Multiple inheritance | Yes (a class can implement many) | No (a class can only inherit one) |
| When to use | Define capabilities ("can do") | Define identity ("is a") |

### Real-World Interface Usage in This Project

```
┌─────────────────────────────────────────────────────────────────┐
│                      PolygonsController                         │
│  Depends on: IPolygonService                                    │
│  Doesn't know about: PolygonService, MongoDB, Repository        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      IPolygonService                            │
│  Contract: GetAll, GetById, Create, Delete                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PolygonService                             │
│  Implements: IPolygonService                                    │
│  Depends on: IPolygonRepository                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    IPolygonRepository                           │
│  Contract: Database operations                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    PolygonRepository                            │
│  Implements: IPolygonRepository                                 │
│  Actually talks to MongoDB                                      │
└─────────────────────────────────────────────────────────────────┘
```

Each layer only knows about the interface above it, not the concrete implementation. This is **loose coupling**.

### Testing with Interfaces

```csharp
// In your test project
public class MockPolygonService : IPolygonService
{
    public List<PolygonDto> MockData { get; set; } = new();
    public bool CreateWasCalled { get; private set; }

    public Task<List<PolygonDto>> GetAllAsync() => Task.FromResult(MockData);

    public Task<PolygonDto> CreateAsync(CreatePolygonRequest request)
    {
        CreateWasCalled = true;  // Track that this was called
        var dto = new PolygonDto { Id = "test-id" };
        MockData.Add(dto);
        return Task.FromResult(dto);
    }

    // ... other methods
}

// In your test
[Fact]
public async Task Create_ShouldAddPolygon()
{
    // Arrange
    var mockService = new MockPolygonService();
    var controller = new PolygonsController(mockService);

    // Act
    await controller.Create(new CreatePolygonRequest { /* ... */ });

    // Assert
    Assert.True(mockService.CreateWasCalled);
    Assert.Single(mockService.MockData);
}
```

### Common .NET Interfaces You'll Encounter

| Interface | Purpose |
|-----------|---------|
| `IEnumerable<T>` | Can be iterated with foreach |
| `ICollection<T>` | Has Count, Add, Remove |
| `IList<T>` | Has indexer (list[0]) |
| `IDisposable` | Has resources to clean up |
| `IAsyncDisposable` | Async cleanup |
| `IEquatable<T>` | Can be compared for equality |
| `IComparable<T>` | Can be sorted |
| `ILogger` | Logging abstraction |
| `IOptions<T>` | Configuration access |

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

**Simple Analogy**: Imagine you're a chef (controller). You need ingredients (dependencies) to cook. There are two ways to get them:

1. **Without DI**: You leave the kitchen, drive to the store, pick ingredients, drive back, then cook. You're doing TWO jobs - shopping AND cooking.

2. **With DI**: Ingredients appear in your kitchen (injected). You just cook. Someone else (the DI container) handles shopping and delivery.

### The Problem: Tight Coupling

Without dependency injection, classes create their own dependencies:

```csharp
public class PolygonsController
{
    private readonly PolygonService _service;

    public PolygonsController()
    {
        // Controller must know HOW to create everything
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "MapServerDb"
        };
        var context = new MongoDbContext(Options.Create(settings));
        var repository = new PolygonRepository(context);
        _service = new PolygonService(repository);  // Finally!
    }
}
```

**Problems with this approach:**
1. **Hard to test**: Can't substitute a mock service
2. **Brittle**: If `PolygonService` constructor changes, this code breaks
3. **Duplicated**: Every class that needs `PolygonService` repeats this setup
4. **Hardcoded config**: Connection string is embedded in code
5. **Wrong responsibility**: Controller's job is HTTP, not wiring dependencies

### The Solution: Dependency Injection

With DI, classes **declare** what they need, and something else **provides** it:

```csharp
public class PolygonsController
{
    private readonly IPolygonService _service;

    // "I need an IPolygonService. I don't care how you make it."
    public PolygonsController(IPolygonService service)
    {
        _service = service;
    }
}
```

The controller is now:
- **Testable**: Inject a mock in tests
- **Flexible**: Swap implementations without changing this code
- **Focused**: Only handles HTTP concerns

### The DI Container: The Matchmaker

ASP.NET Core includes a built-in **DI Container** (also called **IoC Container** - Inversion of Control). It:

1. **Knows recipes**: You tell it "when someone asks for IPolygonService, give them PolygonService"
2. **Creates objects**: When a class needs a dependency, the container creates it
3. **Resolves chains**: If PolygonService needs IRepository, the container creates that too
4. **Manages lifetimes**: Reuses or creates objects based on configuration

### Registration: Teaching the Container

In `Program.cs`, you **register** services:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Tell the container: "When someone asks for IPolygonService, give them PolygonService"
builder.Services.AddScoped<IPolygonService, PolygonService>();

// When someone asks for IPolygonRepository, give them PolygonRepository
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();

// MongoDbContext - concrete class, no interface
builder.Services.AddSingleton<MongoDbContext>();

// Configuration binding
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
```

### Resolution: Getting Dependencies

There are several ways dependencies get resolved:

**1. Constructor Injection (Most Common)**
```csharp
public class PolygonService : IPolygonService
{
    private readonly IPolygonRepository _repository;

    // Container sees: "PolygonService needs IPolygonRepository"
    // Container finds: "IPolygonRepository → PolygonRepository"
    // Container creates PolygonRepository and passes it here
    public PolygonService(IPolygonRepository repository)
    {
        _repository = repository;
    }
}
```

**2. Method Injection (via [FromServices])**
```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromServices] IPolygonService service)
{
    // Service is injected just for this method
    return Ok(await service.GetAllAsync());
}
```

**3. Manual Resolution (Rare, but useful)**
```csharp
public class SomeClass
{
    private readonly IServiceProvider _provider;

    public SomeClass(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void DoSomething()
    {
        // Manually resolve - avoid this pattern when possible
        var service = _provider.GetRequiredService<IPolygonService>();
    }
}
```

### Service Lifetimes: How Long Objects Live

This is **crucial** to understand. Choose wrong, and you'll have bugs or memory leaks.

#### Transient: New Instance Every Time

```csharp
builder.Services.AddTransient<IEmailSender, EmailSender>();
```

```
Request 1: Controller needs IEmailSender → NEW EmailSender #1
           Service also needs IEmailSender → NEW EmailSender #2
           (Two different instances in same request!)

Request 2: Controller needs IEmailSender → NEW EmailSender #3
```

**Use for**: Lightweight, stateless services that don't hold resources.

#### Scoped: One Instance Per HTTP Request

```csharp
builder.Services.AddScoped<IPolygonService, PolygonService>();
```

```
Request 1: Controller needs IPolygonService → NEW PolygonService #1
           Repository also needs it → SAME PolygonService #1
           (Same instance throughout the request)
           Request ends → #1 is disposed

Request 2: Controller needs IPolygonService → NEW PolygonService #2
           (New request, new instance)
```

**Use for**: Database contexts, services with request-specific state, unit-of-work patterns.

**This is the DEFAULT choice for most services.**

#### Singleton: One Instance Forever

```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
```

```
Request 1: Controller needs ICache → NEW MemoryCache #1

Request 2: Controller needs ICache → SAME MemoryCache #1

Request 1000: Controller needs ICache → SAME MemoryCache #1
              (Same instance for the lifetime of the application)
```

**Use for**: Caches, configuration that never changes, expensive-to-create services.

**Warning**: Singletons must be **thread-safe** - multiple requests access them simultaneously.

### Lifetime Visualization

```
Application Starts
│
├── Request 1 ──────────────────────────────────────────────────┐
│   │                                                            │
│   │  Singleton: Cache #1 (shared) ◄────────────────────────┐  │
│   │                                                         │  │
│   │  Scoped: PolygonService #1 (for this request) ◄──────┐ │  │
│   │                                                       │ │  │
│   │  Transient: EmailSender #1 (new)                      │ │  │
│   │  Transient: EmailSender #2 (new)                      │ │  │
│   │                                                       │ │  │
│   │  Request ends: Scoped services disposed ────────────┘ │  │
│   └───────────────────────────────────────────────────────┘  │
│                                                               │
├── Request 2 ──────────────────────────────────────────────────┤
│   │                                                            │
│   │  Singleton: Cache #1 (same as Request 1) ◄─────────────┐  │
│   │                                                         │  │
│   │  Scoped: PolygonService #2 (NEW for this request)      │  │
│   │                                                         │  │
│   │  Transient: EmailSender #3 (new)                        │  │
│   └────────────────────────────────────────────────────────┘  │
│                                                               │
Application Shuts Down: Singletons disposed
```

### The Dependency Chain

The container automatically resolves entire dependency chains:

```csharp
// Registration
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddSingleton<MongoDbContext>();

// When PolygonsController is needed:
// 1. Container sees: PolygonsController needs IPolygonService
// 2. Container sees: IPolygonService → PolygonService
// 3. Container sees: PolygonService needs IPolygonRepository
// 4. Container sees: IPolygonRepository → PolygonRepository
// 5. Container sees: PolygonRepository needs MongoDbContext
// 6. Container creates MongoDbContext (or reuses singleton)
// 7. Container creates PolygonRepository with MongoDbContext
// 8. Container creates PolygonService with PolygonRepository
// 9. Container creates PolygonsController with PolygonService
// 10. All done! Controller is ready to handle the request.
```

### Common Registration Patterns

```csharp
// 1. Interface → Implementation (most common)
builder.Services.AddScoped<IPolygonService, PolygonService>();

// 2. Concrete class only (when there's no interface)
builder.Services.AddSingleton<MongoDbContext>();

// 3. Factory method (when creation needs logic)
builder.Services.AddScoped<IPolygonService>(provider =>
{
    var repo = provider.GetRequiredService<IPolygonRepository>();
    var logger = provider.GetRequiredService<ILogger<PolygonService>>();
    return new PolygonService(repo, logger);
});

// 4. Instance (when you have an existing object)
var myCache = new MySpecialCache();
builder.Services.AddSingleton<ICache>(myCache);

// 5. Self-registration (class registers as itself)
builder.Services.AddScoped<MyService>();  // No interface

// 6. Multiple implementations
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();
// Inject IEnumerable<INotificationService> to get all of them
```

### Common Mistakes and How to Avoid Them

#### Mistake 1: Captive Dependencies

```csharp
// WRONG! Singleton holding Scoped
builder.Services.AddSingleton<MySingleton>();
builder.Services.AddScoped<MyScopedService>();

public class MySingleton
{
    private readonly MyScopedService _scoped;  // BUG!

    public MySingleton(MyScopedService scoped)
    {
        // This scoped service is now "captured" and lives forever
        // It should have been disposed after the first request!
        _scoped = scoped;
    }
}
```

**Rule**: A service can only depend on services with **equal or longer** lifetimes:
- Singleton can use: Singleton only
- Scoped can use: Singleton, Scoped
- Transient can use: Singleton, Scoped, Transient

#### Mistake 2: Forgetting to Register

```csharp
// Forgot to register IPolygonRepository!
builder.Services.AddScoped<IPolygonService, PolygonService>();
// builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();  // Missing!

// Runtime error: "Unable to resolve service for type 'IPolygonRepository'"
```

#### Mistake 3: Service Locator Anti-Pattern

```csharp
// BAD: Using IServiceProvider everywhere (Service Locator pattern)
public class BadService
{
    private readonly IServiceProvider _provider;

    public BadService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void DoWork()
    {
        // Hidden dependencies - hard to test, hard to understand
        var repo = _provider.GetService<IPolygonRepository>();
        var logger = _provider.GetService<ILogger>();
    }
}

// GOOD: Explicit dependencies
public class GoodService
{
    private readonly IPolygonRepository _repo;
    private readonly ILogger _logger;

    public GoodService(IPolygonRepository repo, ILogger<GoodService> logger)
    {
        _repo = repo;
        _logger = logger;
    }
}
```

### Why DI is Essential for Testing

```csharp
// Production: Real services
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();  // Talks to MongoDB

// Test: Mock services
public class PolygonServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsAllPolygons()
    {
        // Arrange - create mock
        var mockRepo = new Mock<IPolygonRepository>();
        mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Polygon>
            {
                new() { Id = "1" },
                new() { Id = "2" }
            });

        // Inject mock - no database needed!
        var service = new PolygonService(mockRepo.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }
}
```

### Quick Reference: This Project's DI Setup

```csharp
// Program.cs

// Configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Database
builder.Services.AddSingleton<MongoDbContext>();  // One connection pool

// Repositories - Scoped (one per request)
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<IMapObjectRepository, MapObjectRepository>();

// Services - Scoped (one per request)
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IMapObjectService, MapObjectService>();

// Controllers - automatically registered by AddControllers()
builder.Services.AddControllers();
```

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

**Simple Analogy**: Attributes are like labels or tags attached to your code. Imagine putting sticky notes on items in a store: "50% OFF", "FRAGILE", "REFRIGERATE". The store (framework) reads these labels and treats items accordingly. The items themselves don't change - they just have extra metadata.

### What Are Attributes?

Attributes are **metadata** - data about data. They don't change what code does directly; they tell frameworks, tools, or other code how to treat that code.

```csharp
// Without attribute - this is just a method
public async Task<List<PolygonDto>> GetAll() { }

// With attribute - ASP.NET Core knows "call this for GET requests"
[HttpGet]
public async Task<List<PolygonDto>> GetAll() { }
```

### How Attributes Work

At compile time, attributes are embedded in the assembly's metadata. At runtime, code can use **reflection** to discover and act on them:

```csharp
// How ASP.NET Core finds your endpoints (simplified):
foreach (var method in controllerType.GetMethods())
{
    var httpGetAttr = method.GetCustomAttribute<HttpGetAttribute>();
    if (httpGetAttr != null)
    {
        // "Aha! This method handles GET requests!"
        RegisterRoute(HttpMethod.Get, httpGetAttr.Template, method);
    }
}
```

### Attribute Syntax

```csharp
// 1. Simple attribute - no parameters
[HttpGet]
public void Get() { }

// 2. Attribute with positional parameter
[HttpGet("all")]  // Template = "all"
public void GetAll() { }

// 3. Attribute with named parameters
[HttpGet(Name = "GetPolygons", Order = 1)]
public void Get() { }

// 4. Multiple attributes
[HttpGet]
[Produces("application/json")]
[ProducesResponseType(200)]
public void Get() { }

// 5. Multiple attributes on one line
[HttpGet, Produces("application/json")]
public void Get() { }

// 6. Attribute with multiple values
[ProducesResponseType(typeof(PolygonDto), 200)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
public void Get(string id) { }
```

### ASP.NET Core Controller Attributes

#### `[ApiController]` - Enables API Behaviors

```csharp
[ApiController]
public class PolygonsController : ControllerBase
```

This single attribute enables several features:

| Feature | Without [ApiController] | With [ApiController] |
|---------|------------------------|---------------------|
| Model validation | Manual: `if (!ModelState.IsValid) return BadRequest()` | Automatic 400 response |
| Parameter binding | Must specify `[FromBody]`, `[FromQuery]` | Automatic inference |
| Problem details | Manual error responses | Standardized RFC 7807 format |
| Route requirement | Optional | Required (must have `[Route]`) |

```csharp
// WITHOUT [ApiController] - must do this manually
[HttpPost]
public IActionResult Create([FromBody] CreatePolygonRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // ... create logic
}

// WITH [ApiController] - automatic
[HttpPost]
public IActionResult Create(CreatePolygonRequest request)  // [FromBody] inferred
{
    // If request is invalid, framework automatically returns 400
    // We only get here if validation passes
}
```

#### `[Route]` - URL Pattern Matching

```csharp
// Class-level route - base URL for all actions
[Route("api/[controller]")]  // [controller] = class name minus "Controller"
public class PolygonsController  // → /api/polygons

[Route("api/v1/geo/polygons")]  // Explicit route
public class PolygonsController  // → /api/v1/geo/polygons
```

**Route tokens:**
- `[controller]` → Controller name without "Controller" suffix
- `[action]` → Method name
- `[area]` → Area name (if using areas)

```csharp
[Route("api/[controller]/[action]")]
public class DataController
{
    public void Export() { }    // → /api/data/export
    public void Import() { }    // → /api/data/import
}
```

#### HTTP Method Attributes

```csharp
public class PolygonsController : ControllerBase
{
    // GET /api/polygons
    [HttpGet]
    public IActionResult GetAll() { }

    // GET /api/polygons/abc123
    [HttpGet("{id}")]
    public IActionResult GetById(string id) { }

    // POST /api/polygons
    [HttpPost]
    public IActionResult Create(CreatePolygonRequest request) { }

    // PUT /api/polygons/abc123
    [HttpPut("{id}")]
    public IActionResult Update(string id, UpdateRequest request) { }

    // DELETE /api/polygons/abc123
    [HttpDelete("{id}")]
    public IActionResult Delete(string id) { }
}
```

**Route parameters:**

```csharp
// Required parameter
[HttpGet("{id}")]
public IActionResult Get(string id) { }  // /api/polygons/123

// Optional parameter
[HttpGet("{id?}")]
public IActionResult Get(string? id) { }  // /api/polygons or /api/polygons/123

// Constrained parameter
[HttpGet("{id:int}")]
public IActionResult Get(int id) { }  // Only matches integers

[HttpGet("{id:guid}")]
public IActionResult Get(Guid id) { }  // Only matches GUIDs

[HttpGet("{id:length(24)}")]
public IActionResult Get(string id) { }  // Only matches 24-char strings (MongoDB ObjectId)

// Multiple parameters
[HttpGet("{year}/{month}/{day}")]
public IActionResult GetByDate(int year, int month, int day) { }
// /api/polygons/2024/01/15
```

#### Parameter Binding Attributes

```csharp
public class PolygonsController : ControllerBase
{
    // From URL route: /api/polygons/123
    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] string id) { }

    // From query string: /api/polygons?page=1&size=10
    [HttpGet]
    public IActionResult GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10) { }

    // From request body (JSON)
    [HttpPost]
    public IActionResult Create([FromBody] CreatePolygonRequest request) { }

    // From request headers
    [HttpGet]
    public IActionResult Get([FromHeader(Name = "X-Api-Key")] string apiKey) { }

    // From form data
    [HttpPost]
    public IActionResult Upload([FromForm] IFormFile file) { }

    // From DI container
    [HttpGet]
    public IActionResult Get([FromServices] IPolygonService service) { }
}
```

### MongoDB/BSON Attributes

These control how C# objects are stored in MongoDB:

```csharp
public class Polygon
{
    // This is the document's _id field
    [BsonId]
    // Store as ObjectId type, but expose as string in C#
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Store this property as "geo" in MongoDB (not "Geometry")
    [BsonElement("geo")]
    public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Geometry { get; set; }

    // Don't store this property in MongoDB at all
    [BsonIgnore]
    public bool IsSelected { get; set; }

    // Only store if not null
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    // Use specific date format
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
}
```

**Before/After comparison in MongoDB:**

```javascript
// C# class stores this:
{
    "_id": ObjectId("507f1f77bcf86cd799439011"),  // [BsonId]
    "geo": { ... },                                 // [BsonElement("geo")]
    "createdAt": ISODate("2024-01-15T10:30:00Z")  // [BsonDateTimeOptions]
    // IsSelected is NOT stored ([BsonIgnore])
}
```

### Validation Attributes

```csharp
public class CreatePolygonRequest
{
    [Required(ErrorMessage = "Coordinates are required")]
    [MinLength(3, ErrorMessage = "A polygon needs at least 3 coordinates")]
    public List<Coordinate> Coordinates { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [EmailAddress]
    public string? CreatedBy { get; set; }

    [RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string? Code { get; set; }
}
```

With `[ApiController]`, validation is automatic:

```csharp
// Request with missing coordinates
POST /api/polygons
{ "name": "Test" }

// Automatic 400 response:
{
    "errors": {
        "Coordinates": ["Coordinates are required"]
    }
}
```

### Creating Custom Attributes

You can create your own attributes:

```csharp
// Define the attribute
[AttributeUsage(AttributeTargets.Property)]  // Can only be used on properties
public class ValidCoordinateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not Coordinate coord)
            return new ValidationResult("Must be a Coordinate");

        if (coord.Latitude < -90 || coord.Latitude > 90)
            return new ValidationResult("Latitude must be between -90 and 90");

        if (coord.Longitude < -180 || coord.Longitude > 180)
            return new ValidationResult("Longitude must be between -180 and 180");

        return ValidationResult.Success;
    }
}

// Use it
public class Marker
{
    [ValidCoordinate]
    public Coordinate Location { get; set; }
}
```

### AttributeUsage: Controlling Where Attributes Apply

```csharp
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,  // Where it can be used
    AllowMultiple = false,                             // Can only appear once
    Inherited = true                                   // Inherited by derived classes
)]
public class MyCustomAttribute : Attribute
{
}
```

**AttributeTargets options:**
- `Class` - Can be applied to classes
- `Method` - Can be applied to methods
- `Property` - Can be applied to properties
- `Parameter` - Can be applied to method parameters
- `Field` - Can be applied to fields
- `All` - Can be applied anywhere

### Common Attribute Patterns in This Project

```csharp
// Controller with full attribute stack
[ApiController]                           // Enable API behaviors
[Route("api/[controller]")]               // Base route: /api/polygons
[Produces("application/json")]            // Always returns JSON
public class PolygonsController : ControllerBase
{
    private readonly IPolygonService _service;

    // Constructor injection (no attributes needed here)
    public PolygonsController(IPolygonService service)
    {
        _service = service;
    }

    // GET /api/polygons
    [HttpGet]
    [ProducesResponseType(typeof(List<PolygonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PolygonDto>>> GetAll()
    {
        return await _service.GetAllAsync();
    }

    // GET /api/polygons/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PolygonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolygonDto>> GetById(string id)
    {
        var polygon = await _service.GetByIdAsync(id);
        if (polygon == null)
            return NotFound();
        return polygon;
    }

    // POST /api/polygons
    [HttpPost]
    [ProducesResponseType(typeof(PolygonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PolygonDto>> Create(CreatePolygonRequest request)
    {
        var polygon = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = polygon.Id }, polygon);
    }
}
```

### Attributes vs Configuration

Attributes are just ONE way to configure behavior. Same result, different style:

```csharp
// APPROACH 1: Attributes (declarative)
[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() { }
}

// APPROACH 2: Convention-based (in Program.cs)
app.MapGet("/api/polygons", () => { /* handler */ });

// APPROACH 3: Fluent configuration
endpoints.MapControllerRoute(
    name: "polygons",
    pattern: "api/polygons",
    defaults: new { controller = "Polygons", action = "GetAll" });
```

Attributes are preferred for controllers because they're **co-located with the code** - you see the route right where the method is defined.

### Quick Reference: Attribute Categories

| Category | Examples | Purpose |
|----------|----------|---------|
| **Routing** | `[Route]`, `[HttpGet]`, `[HttpPost]` | URL → Method mapping |
| **Binding** | `[FromBody]`, `[FromQuery]`, `[FromRoute]` | Where to get parameter values |
| **Validation** | `[Required]`, `[Range]`, `[StringLength]` | Input validation rules |
| **Documentation** | `[Produces]`, `[ProducesResponseType]` | OpenAPI/Swagger docs |
| **Serialization** | `[JsonPropertyName]`, `[BsonElement]` | JSON/BSON field mapping |
| **Security** | `[Authorize]`, `[AllowAnonymous]` | Access control |
| **Behavior** | `[ApiController]`, `[NonAction]` | Framework features |

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
