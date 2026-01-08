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
docker-compose up -d
```

Or connect to an existing MongoDB instance by updating `MapServer/appsettings.json`.

### 3. Run the Backend

```bash
cd MapServer
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5102`.

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
├── MapClient/          # React frontend
│   ├── src/
│   │   ├── api/        # API client and endpoints
│   │   ├── components/ # Reusable UI components
│   │   ├── context/    # React context (MapContext)
│   │   ├── features/   # Feature modules (map, polygons, objects)
│   │   └── types/      # TypeScript types
│   └── ...
├── MapServer/          # ASP.NET Core backend
│   ├── Controllers/    # API endpoints
│   ├── Services/       # Business logic
│   ├── Repositories/   # Data access layer
│   ├── Models/         # MongoDB document models
│   ├── DTOs/           # Request/response objects
│   └── ...
└── README.md
```

## Features

- **Draw Polygons**: Click on the map to draw polygon vertices, click near the first point to close
- **Place Markers**: Select an object type and click on the map to place markers
- **View Data**: See all polygons and objects in the data table panel
- **Delete Items**: Remove individual items or clear all data

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/polygons` | Get all polygons |
| GET | `/api/polygons/{id}` | Get polygon by ID |
| POST | `/api/polygons` | Create polygon |
| DELETE | `/api/polygons/{id}` | Delete polygon |
| DELETE | `/api/polygons` | Delete all polygons |
| GET | `/api/objects` | Get all map objects |
| GET | `/api/objects/{id}` | Get object by ID |
| POST | `/api/objects` | Create single object |
| POST | `/api/objects/batch` | Create multiple objects |
| DELETE | `/api/objects/{id}` | Delete object |
| DELETE | `/api/objects` | Delete all objects |

## Available Scripts

### Frontend (MapClient)

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production |
| `npm run lint` | Run ESLint |
| `npm run preview` | Preview production build |

### Backend (MapServer)

| Command | Description |
|---------|-------------|
| `dotnet run` | Run the server |
| `dotnet build` | Build the project |
| `dotnet restore` | Restore dependencies |

## Configuration

### Backend (appsettings.json)

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
