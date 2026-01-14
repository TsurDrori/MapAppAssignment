# MapServer - ASP.NET Core REST API

ASP.NET Core 10 REST API backend for a map application that manages geographic objects (polygons and markers) using MongoDB + GeoJSON.

## Quick Start

### Prerequisites

- .NET 10 SDK
- MongoDB (see setup options below)

### MongoDB Setup Options

#### Option 1: Docker (Recommended - easiest)

```bash
# From MapServer/

# Start MongoDB
docker compose up -d

# Stop MongoDB
docker compose down

# Stop and remove data
docker compose down -v
```

#### Option 2: Docker command line

```bash
docker run -d --name mapapp-mongodb -p 27017:27017 mongo:latest
```

#### Option 3: Local MongoDB installation

1. Download from [mongodb.com/download-center/community](https://www.mongodb.com/try/download/community)
2. Install and run MongoDB locally
3. Default connection: `mongodb://localhost:27017`

### Running the Server

```bash
dotnet restore

# Windows (PowerShell)
.\build.cmd
.\run.cmd

# macOS/Linux (bash)
./build.sh
./run.sh
```

Hot reload: `.\watch.cmd` (Windows) / `./watch.sh` (macOS/Linux)

Or use the full commands:

```bash
dotnet build MapServer.slnx
dotnet run --project MapServer.Api
dotnet run --project MapServer.Api --launch-profile https  # HTTPS
```

The server will be available at:

- **HTTP**: `http://localhost:5102`
- **HTTPS**: `https://localhost:7103`
- **Swagger UI** (dev only): `http://localhost:5102/swagger`

## Testing the API

### Using VS Code REST Client

1. Open [MapServer.http](MapServer.http) in VS Code
2. Install the "REST Client" extension if needed
3. Click "Send Request" above any request
4. Test the complete workflow:
   - Create polygons and objects
   - Copy the returned IDs
   - Update `@polygonId` and `@objectId` variables at the top
   - Test GET by ID and DELETE operations

### Using curl

```bash
# Get all polygons
curl http://localhost:5102/api/polygons

# Create a polygon
curl -X POST http://localhost:5102/api/polygons \
  -H "Content-Type: application/json" \
  -d '{
    "coordinates": [
      { "latitude": 32.08, "longitude": 34.78 },
      { "latitude": 32.08, "longitude": 34.79 },
      { "latitude": 32.09, "longitude": 34.79 }
    ]
  }'

# Create a map object
curl -X POST http://localhost:5102/api/objects \
  -H "Content-Type: application/json" \
  -d '{
    "location": { "latitude": 32.08, "longitude": 34.78 },
    "objectType": "Marker"
  }'
```

## API Endpoints

### Polygons

| Method | Endpoint             | Description       |
| ------ | -------------------- | ----------------- |
| GET    | `/api/polygons`      | Get all polygons  |
| GET    | `/api/polygons/{id}` | Get polygon by ID |
| POST   | `/api/polygons`      | Create polygon    |
| DELETE | `/api/polygons/{id}` | Delete polygon    |

### Map Objects

| Method | Endpoint             | Description             |
| ------ | -------------------- | ----------------------- |
| GET    | `/api/objects`       | Get all map objects     |
| GET    | `/api/objects/{id}`  | Get object by ID        |
| POST   | `/api/objects`       | Create single object    |
| POST   | `/api/objects/batch` | Create multiple objects |
| DELETE | `/api/objects/{id}`  | Delete object           |

## Project Structure

Clean Architecture with multi-project solution:

```
MapServer/
  MapServer.slnx                    # Solution file
  docker-compose.yml                # MongoDB container
  MapServer.http                    # API testing file

  MapServer.Api/                    # Presentation layer
    Controllers/                    # HTTP endpoint handlers
    Middleware/                     # Global exception mapping
    Program.cs                      # Entry point, DI configuration
    appsettings.json                # Configuration

  MapServer.Application/            # Application layer (use cases)
    DTOs/                           # Request/response objects
    Interfaces/                     # Service interfaces
    Services/                       # Business logic + guard validation

  MapServer.Domain/                 # Domain layer (core)
    Entities/                       # Domain models
    Exceptions/                     # Domain exceptions
    ValueObjects/                   # Value objects (GeoCoordinate, etc.)

  MapServer.Infrastructure/         # Infrastructure layer
    Configuration/                  # MongoDbSettings
    Data/                           # MongoDbContext (connection + indexes)
    Documents/                      # MongoDB document models
    Mapping/                        # Domain <-> document mapping
    Repositories/                   # Repository implementations
```

### Project Dependencies

Following Dependency Inversion Principle:

- **Api** -> Application, Infrastructure (for DI wiring)
- **Application** -> Domain (business logic uses domain abstractions)
- **Infrastructure** -> Domain (implements repository interfaces)
- **Domain** -> No dependencies (core layer)

## Configuration

MongoDB settings are in [MapServer.Api/appsettings.json](MapServer.Api/appsettings.json):

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

## Features

- **Clean Architecture**: Service + repository pattern with clear separation
- **GeoJSON Support**: MongoDB GeoJSON types with 2dsphere spatial indexes (created on startup)
- **Validation**:
  - DTO validation via `DataAnnotations` (ASP.NET Core returns `ValidationProblemDetails`)
  - Guard validation in services for consistent behavior
  - Polygon self-intersection/loop validity is enforced by MongoDB when writing/indexing GeoJSON
- **Auto-closing Polygons**: Server ensures polygons are closed per GeoJSON spec
- **Error Handling**: RFC 7807 `ProblemDetails` (`application/problem+json`)
- **CORS**: Configured for React frontend (ports 3000, 5173)
- **Async/Await**: All database operations are asynchronous

## Data Models

### Polygon request

```json
{
  "coordinates": [
    { "latitude": 32.08, "longitude": 34.78 },
    { "latitude": 32.08, "longitude": 34.79 },
    { "latitude": 32.09, "longitude": 34.79 }
  ]
}
```

### Map object request

```json
{
  "location": { "latitude": 32.08, "longitude": 34.78 },
  "objectType": "Marker"
}
```

### Batch create objects

```json
{
  "objects": [
    {
      "location": { "latitude": 32.11, "longitude": 34.81 },
      "objectType": "Marker"
    },
    {
      "location": { "latitude": 32.12, "longitude": 34.82 },
      "objectType": "Jeep"
    }
  ]
}
```

## Validation Rules

- **Polygons**:
  - Minimum 3 coordinates (server auto-closes if needed)
  - Must be a valid GeoJSON polygon loop (MongoDB rejects invalid/self-intersecting loops)
- **Coordinates**:
  - Latitude: -90 to 90
  - Longitude: -180 to 180
- **ObjectType**: Required, any non-empty string (no enum/list enforced)

## Development

### Checking MongoDB

```bash
# Connect to MongoDB container
docker exec -it mapapp-mongodb mongosh

# List databases
show dbs

# Use the MapApp database
use MapAppDb

# Show collections
show collections

# Query polygons
db.Polygons.find().pretty()

# Query objects
db.MapObjects.find().pretty()

# Exit
exit
```

### Rebuilding

```bash
dotnet clean MapServer.slnx
dotnet restore

# Windows (PowerShell)
.\build.cmd

# macOS/Linux (bash)
./build.sh
```

## Troubleshooting

### MongoDB Connection Issues

- Verify MongoDB is running: `docker ps` or check local MongoDB service
- Check connection string in `MapServer.Api/appsettings.json`
- Ensure port 27017 is not blocked

### Port Already in Use

- Change ports in [MapServer.Api/Properties/launchSettings.json](MapServer.Api/Properties/launchSettings.json)
- Update `@MapServer_HostAddress` in [MapServer.http](MapServer.http)

### CORS Errors from React

- Verify React app port list in [MapServer.Api/Program.cs](MapServer.Api/Program.cs)
- Check that `UseCors` is called before `MapControllers()`
