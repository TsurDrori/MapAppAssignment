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
├── MapClient/                    # React frontend
│   ├── src/
│   │   ├── api/                  # API client and endpoints
│   │   ├── components/           # Reusable UI components
│   │   ├── context/              # React context (MapContext)
│   │   ├── features/             # Feature modules (map, polygons, objects)
│   │   └── types/                # TypeScript types
│   └── ...
├── MapServer/                    # ASP.NET Core backend (Clean Architecture)
│   ├── MapServer.slnx            # Solution file
│   ├── MapServer.Api/            # Presentation layer (Controllers, Program.cs)
│   ├── MapServer.Application/    # Application layer (Services, DTOs)
│   ├── MapServer.Domain/         # Domain layer (Entities, Repository interfaces)
│   ├── MapServer.Infrastructure/ # Infrastructure layer (Repositories, DbContext)
│   └── ...
└── README.md
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

| Command          | Description              |
| ---------------- | ------------------------ |
| `./run`          | Run the server           |
| `./build`        | Build all projects       |
| `./watch`        | Run with hot reload      |
| `dotnet restore` | Restore dependencies     |

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

### Backend Architecture

The server follows **Clean Architecture** with a multi-project solution structure:

| Project        | Layer          | Responsibility                                  |
| -------------- | -------------- | ----------------------------------------------- |
| MapServer.Api  | Presentation   | HTTP routing, DI configuration (thin controllers) |
| MapServer.Application | Application | Business logic, validation, DTOs |
| MapServer.Domain | Domain       | Entities, repository interfaces (core abstractions) |
| MapServer.Infrastructure | Infrastructure | Data access, MongoDB operations |

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

### Frontend Architecture

- **Feature-based structure** - code organized by feature (map, polygons, objects) not by type
- **Split Context pattern** - separate state/dispatch contexts to prevent unnecessary re-renders
- **React Query for server state** - automatic caching, refetching, loading/error states
- **Custom hooks** (`usePolygons`, `useObjects`) - encapsulate data fetching logic

### Coordinate Handling

The API uses explicit `{ latitude, longitude }` objects rather than arrays because:

- GeoJSON uses `[longitude, latitude]` order
- Leaflet uses `[latitude, longitude]` order
- Named properties prevent accidental coordinate swaps

Conversion happens at the repository layer, keeping the API clean.

For comprehensive design rationale, see [MapServer/ARCHITECTURE_DECISIONS.md](MapServer/ARCHITECTURE_DECISIONS.md).

## Documentation

- [ARCHITECTURE_DECISIONS.md](MapServer/ARCHITECTURE_DECISIONS.md) - Detailed rationale for every architectural choice, with alternatives considered
- [BACKEND_CONCEPTS.md](MapServer/BACKEND_CONCEPTS.md) - Backend programming concepts explained in plain English
