# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MapServer is an ASP.NET Core 10 REST API backend for a map application that manages geographic objects (Polygons and Markers). It uses MongoDB with GeoJSON format for data storage.

Check assinment.txt for source of truth details on the requirments of this project

## Build & Run Commands

```bash
# Run the server (development)
dotnet run

# Run with specific profile
dotnet run --launch-profile https

# Build
dotnet build

# Restore packages
dotnet restore
```

## Development URLs

- HTTP: `http://localhost:5102`
- HTTPS: `https://localhost:7103`
- OpenAPI (dev only): `/openapi/v1.json`

## Architecture

Clean Architecture (Onion) with four layers:

```
Presentation (Controllers)
       ↓
Application (Services, DTOs, Interfaces)
       ↓
Domain (Entities, Value Objects)
       ↓
Infrastructure (MongoDB Repositories)
```

### Target Project Structure

```
MapServer/
├── Controllers/        # Thin - routing + HTTP concerns only
├── Services/          # Business logic + validation (interfaces + implementations)
├── Repositories/      # Data access (explicit interfaces per entity)
├── Models/            # Polygon, MapObject (simple entities)
├── DTOs/              # Request/response objects
├── Data/              # MongoDB context
└── Configuration/     # Settings classes
```

### Architecture Approach

- **Service + Repository pattern** for separation of concerns
- **Explicit interfaces per entity** (no generic `IRepository<T>`)
- **Validation in services** (no separate validator classes)
- **Simple entities** (no base interface - YAGNI)
- **Controllers depend on service interfaces** for testability

### API Endpoints

| Method | Endpoint             | Description                              |
| ------ | -------------------- | ---------------------------------------- |
| GET    | `/api/polygons`      | Get all polygons                         |
| GET    | `/api/polygons/{id}` | Get polygon by ID                        |
| POST   | `/api/polygons`      | Create polygon                           |
| DELETE | `/api/polygons/{id}` | Delete polygon                           |
| DELETE | `/api/polygons`      | Delete all polygons                      |
| GET    | `/api/objects`       | Get all map objects                      |
| GET    | `/api/objects/{id}`  | Get object by ID                         |
| POST   | `/api/objects`       | Create single object                     |
| POST   | `/api/objects/batch` | Create multiple objects (per assignment) |
| DELETE | `/api/objects/{id}`  | Delete object                            |
| DELETE | `/api/objects`       | Delete all objects                       |

## Key Dependencies

- **MongoDB.Driver 3.5.2**: Database access, use GeoJSON types for spatial data
- **Microsoft.AspNetCore.OpenApi 10.0.1**: API documentation

## Data Model Guidelines

### DTO Layer (API Contracts)

Use simple, technology-agnostic models for API requests/responses:

- **DTOs use simple `Coordinate` class**: `{ Latitude: double, Longitude: double }`
- **Polygon DTOs**: List of `Coordinate` objects
- **Object DTOs**: `Coordinate` + `ObjectType` string
- **ObjectType**: Unrestricted string (e.g., "Marker", "Jeep") - client determines valid types

### Repository Layer (MongoDB Documents)

Convert to MongoDB GeoJSON types at repository boundary:

- **Polygon geometry**: `GeoJsonPolygon<GeoJson2DGeographicCoordinates>`
- **Object location**: `GeoJsonPoint<GeoJson2DGeographicCoordinates>`
- Repository handles conversion between DTO and MongoDB models
- Keeps domain layer clean and database-agnostic

### MongoDB Considerations

- **Coordinate Order**: API uses GeoJSON standard `[Longitude, Latitude]` - frontend handles conversion from Leaflet's `[Lat, Lng]`
- **Spatial Index**: Create `2dsphere` index on geometry fields for geo queries
- **Polygon Auto-Closure**: Server ensures polygons are closed (adds final coordinate matching first if missing) - defensive against client bugs while maintaining GeoJSON validity

## Design Principles

- **Stateless API**: No server-side session state, enables horizontal scaling
- **Async I/O**: All database operations use async/await
- **Validation in services**: Simple, visible, no separate validator classes
- **Error responses**: Use RFC 7807 Problem Details format for consistent error handling
- **No UPDATE operations**: Assignment specifies Create/Delete only - edit via delete + recreate

### Validation Rules

- **Polygons**: Minimum 3 coordinates, auto-closed by server
- **Objects**: Coordinate validation only (lat: -90 to 90, lon: -180 to 180)
- **ObjectType**: No validation - any string accepted (client controls valid types)

## Related Components

This server is part of a larger MapApp. The client is a React application (separate project) using Leaflet for map rendering.
