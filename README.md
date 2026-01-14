# MapApp

A full-stack map application for managing geographic objects (polygons and markers) built with React and ASP.NET Core.

## Tech Stack

**Frontend:**

- React 19 with TypeScript
- Leaflet / React-Leaflet for map rendering
- TanStack Query for server state management
- Tailwind CSS for styling
- Vite for build tooling

**Backend:**

- ASP.NET Core 10 REST API
- MongoDB with GeoJSON for spatial data
- Clean Architecture (Service + Repository pattern)

## Prerequisites

- [Node.js](https://nodejs.org/) (v18+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for MongoDB) or local MongoDB installation

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd MapApp
```

### 2. Start MongoDB

Using Docker (recommended):

```bash
cd MapServer
docker compose up -d
```

Or connect to an existing MongoDB instance by updating `MapServer/MapServer.Api/appsettings.json`.

### 3. Run the Backend

```bash
cd MapServer
dotnet restore
./build
./run
```

Or with hot reload: `./watch`

The API will be available at `http://localhost:5102`.

Swagger UI is available at `http://localhost:5102/swagger` in development mode.

### 4. Run the Frontend

```bash
cd MapClient
npm install
npm run dev
```

The application will be available at `http://localhost:5173`.

## Project Structure

```
MapApp/
  MapClient/                    # React frontend
    src/
      api/                      # API client and endpoints
      components/               # Reusable UI components
      context/                  # React context (MapContext)
      features/                 # Feature modules (map, polygons, objects)
      types/                    # TypeScript types
  MapServer/                    # ASP.NET Core backend (Clean Architecture)
    MapServer.slnx              # Solution file
    MapServer.Api/              # Presentation layer (Controllers, Program.cs)
    MapServer.Application/      # Application layer (Services, DTOs)
    MapServer.Domain/           # Domain layer (Entities, repository interfaces)
    MapServer.Infrastructure/   # Infrastructure layer (Repositories, MongoDbContext)
  README.md
```

## Features

- **Draw Polygons**: Click on the map to draw polygon vertices, click near the first point to close
- **Place Markers**: Select an object type and click on the map to place markers
- **View Data**: See all polygons and objects in the data table panel
- **Delete Items**: Remove individual items or clear all data

## API Documentation

Interactive API documentation is available via **Swagger UI** at:

```
http://localhost:5102/swagger
```

This provides a full interactive interface to explore and test all API endpoints.

### Endpoints Summary

| Method | Endpoint             | Description             |
| ------ | -------------------- | ----------------------- |
| GET    | `/api/polygons`      | Get all polygons        |
| GET    | `/api/polygons/{id}` | Get polygon by ID       |
| POST   | `/api/polygons`      | Create polygon          |
| DELETE | `/api/polygons/{id}` | Delete polygon          |
| GET    | `/api/objects`       | Get all map objects     |
| GET    | `/api/objects/{id}`  | Get object by ID        |
| POST   | `/api/objects`       | Create single object    |
| POST   | `/api/objects/batch` | Create multiple objects |
| DELETE | `/api/objects/{id}`  | Delete object           |

## Available Scripts

### Frontend (MapClient)

| Command           | Description              |
| ----------------- | ------------------------ |
| `npm run dev`     | Start development server |
| `npm run build`   | Build for production     |
| `npm run lint`    | Run ESLint               |
| `npm run preview` | Preview production build |

### Backend (MapServer)

| Command          | Description          |
| ---------------- | -------------------- |
| `./run`          | Run the server       |
| `./build`        | Build all projects   |
| `./watch`        | Run with hot reload  |
| `dotnet restore` | Restore dependencies |

## Configuration

### Backend (MapServer.Api/appsettings.json)

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MapAppDb",
    "PolygonsCollectionName": "Polygons",
    "ObjectsCollectionName": "MapObjects"
  }
}
```

### Frontend

The Vite dev server proxies `/api` requests to `http://localhost:5102`. This can be changed in `vite.config.ts`.

## Architecture & Design Decisions

### Backend (MapServer)

The server follows **Clean Architecture** with a multi-project solution structure:

| Project                  | Layer          | Responsibility                                      |
| ------------------------ | -------------- | --------------------------------------------------- |
| MapServer.Api            | Presentation   | HTTP routing, DI configuration (thin controllers)   |
| MapServer.Application    | Application    | Business logic, validation, DTOs                    |
| MapServer.Domain         | Domain         | Entities, repository interfaces (core abstractions) |
| MapServer.Infrastructure | Infrastructure | Data access, MongoDB operations                     |

**Project Dependencies (Dependency Inversion Principle):**

- Api → Application, Infrastructure
- Application → Domain
- Infrastructure → Domain
- Domain → No dependencies

**Key Design Choices:**

- **Multi-project solution** - enforces architectural boundaries at compile time
- **Repository interfaces in Domain** - Application depends on abstractions, not implementations
- **Explicit repository interfaces** per entity rather than generic `IRepository<T>` - allows entity-specific methods like geo queries
- **Service-level validation** - keeps validation visible and simple for this project scope
- **DTO pattern** - separates API contracts from database models, making the API database-agnostic
- **Separate batch endpoint** (`/objects/batch`) - explicit contracts rather than polymorphic endpoints
- **Coordinate handling** - API uses explicit `{ latitude, longitude }` objects (not arrays) to prevent GeoJSON/Leaflet coordinate order confusion; conversion happens at the repository layer

For comprehensive backend design rationale, see [MapServer/ARCHITECTURE_DECISIONS.md](MapServer/ARCHITECTURE_DECISIONS.md).

### Frontend (MapClient)

The React client demonstrates modern frontend patterns:

| Pattern                     | Implementation                                     | Benefit                                                 |
| --------------------------- | -------------------------------------------------- | ------------------------------------------------------- |
| **Feature-based structure** | Code organized by feature (map, polygons, objects) | Scalability, co-location of related code                |
| **Split Context pattern**   | Separate state/dispatch contexts                   | Prevents unnecessary re-renders                         |
| **React Query**             | Server state with automatic caching                | Eliminates boilerplate, consistent loading/error states |
| **Custom hooks**            | `usePolygons`, `useObjects` with React Query       | Encapsulated data fetching, reusable logic              |
| **Memoization**             | `React.memo`, `useMemo` for icons/components       | Prevents expensive re-renders                           |
| **Lazy loading**            | `React.lazy` for non-critical panels               | Reduced initial bundle size                             |
| **Error boundaries**        | Class components wrapping feature groups           | Graceful degradation                                    |
| **Type-safe actions**       | Discriminated unions for reducer actions           | Compile-time action validation                          |
