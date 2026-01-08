# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MapApp is a full-stack map application for managing geographic objects (polygons and markers). It consists of two projects:

- **MapServer/**: ASP.NET Core 10 REST API backend using MongoDB with GeoJSON
- **MapClient/**: React 19 + TypeScript frontend using Leaflet for map rendering

## Build & Run Commands

### MapServer (Backend)
```bash
cd MapServer
dotnet restore          # Restore packages
dotnet build            # Build
dotnet run              # Run on http://localhost:5102
```

### MapClient (Frontend)
```bash
cd MapClient
npm install             # Install dependencies
npm run dev             # Run dev server on http://localhost:5173
npm run build           # Build: tsc -b && vite build
npm run lint            # Run ESLint
```

### MongoDB (Required for backend)
```bash
cd MapServer
docker-compose up -d    # Start MongoDB container
docker-compose down     # Stop MongoDB
```

## Architecture

### Backend (MapServer)

Clean Architecture with Service + Repository pattern:

```
Controllers/     → Thin - routing + HTTP concerns only
Services/        → Business logic + validation (IPolygonService, IMapObjectService)
Repositories/    → Data access (IPolygonRepository, IMapObjectRepository)
Models/          → MongoDB document entities
DTOs/            → API request/response objects
Data/            → MongoDbContext
```

Key design decisions:
- No UPDATE operations - edit via delete + recreate (per requirements)
- Server auto-closes polygons if needed (adds first coordinate to end)
- ObjectType is unrestricted string - client determines valid types
- Coordinate validation: lat -90 to 90, lon -180 to 180

### Frontend (MapClient)

Feature-based structure with React Query for server state:

```
src/
├── api/           → API client (fetch wrapper) + endpoint modules
├── context/       → MapContext (useReducer for UI state: mode, selections, pending items)
├── features/
│   ├── map/       → MapView, PolygonLayer, ObjectLayer, DrawingLayer
│   ├── polygons/  → PolygonPanel, usePolygons hook
│   ├── objects/   → ObjectPanel, ObjectTypeSelector, useObjects hook
│   └── data-table/→ DataTablePanel
├── components/ui/ → Reusable Button, Panel, Table
├── types/         → TypeScript API types
└── config/        → Map config, marker types
```

Key patterns:
- `@/` path alias maps to `src/`
- MapContext uses split contexts (state/dispatch) to prevent re-renders
- React Query manages server state with 1-minute stale time
- Vite proxies `/api` requests to backend at localhost:5102

### Coordinate Systems

- **API uses GeoJSON standard**: `[longitude, latitude]` order
- **Leaflet uses**: `[latitude, longitude]` order
- **DTOs use**: `{ latitude: number, longitude: number }` object format
- Frontend handles conversion between Leaflet and API formats

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET/POST/DELETE | `/api/polygons` | Polygon CRUD |
| GET/DELETE | `/api/polygons/{id}` | Single polygon |
| GET/POST/DELETE | `/api/objects` | Map object CRUD |
| GET/DELETE | `/api/objects/{id}` | Single object |
| POST | `/api/objects/batch` | Batch create objects |

## Testing the API

Use the REST Client extension with `MapServer/MapServer.http` for manual API testing.
