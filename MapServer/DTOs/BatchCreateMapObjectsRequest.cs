// ============================================================================
// BatchCreateMapObjectsRequest.cs - DATA FOR CREATING MULTIPLE OBJECTS AT ONCE
// ============================================================================
//
// WHAT: The data structure for creating multiple map objects in a single request.
//       This is a "batch" or "bulk" operation.
//
// WHY:  Instead of calling POST /api/objects 100 times (100 network requests),
//       the client can call POST /api/objects/batch once with 100 objects.
//       This is MUCH faster because:
//       - Only 1 network round-trip instead of 100
//       - MongoDB can insert all documents in one operation
//       - Less overhead per object
//
// EXAMPLE REQUEST BODY:
//       POST /api/objects/batch
//       {
//         "objects": [
//           { "location": { "latitude": 32.5, "longitude": 35.2 }, "objectType": "Marker" },
//           { "location": { "latitude": 32.6, "longitude": 35.3 }, "objectType": "Jeep" },
//           { "location": { "latitude": 32.7, "longitude": 35.4 }, "objectType": "Tank" }
//         ]
//       }
//
// VALIDATION:
//       Each object in the list is validated individually.
//       If ANY object is invalid, the ENTIRE batch fails.
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects)
// ============================================================================

namespace MapServer.DTOs;

// ============================================================================
// BatchCreateMapObjectsRequest CLASS - Request DTO for batch creation
// ============================================================================
public class BatchCreateMapObjectsRequest
{
    // ========================================================================
    // Objects - The list of objects to create
    // ========================================================================
    // Each item in this list is a CreateMapObjectRequest (has Location and ObjectType).
    // All objects will be created in a single database operation.
    //
    // "List<CreateMapObjectRequest>" = a list where each item is a CreateMapObjectRequest
    //   - This is composition: using one DTO inside another
    //   - Avoids duplicating the Location/ObjectType definition
    //
    // "= new()" = start with an empty list (never null)
    //
    // THE FLOW:
    //   1. Client sends JSON with "objects" array
    //   2. ASP.NET Core deserializes JSON into this class
    //   3. Controller receives this object
    //   4. Service validates each object
    //   5. Service converts all to MongoDB models
    //   6. Repository inserts all in one database call
    //   7. Service converts results back to DTOs
    //   8. Controller returns the created objects with their new IDs
    //
    // See BACKEND_CONCEPTS.md: Generic Types
    // ========================================================================
    public List<CreateMapObjectRequest> Objects { get; set; } = new();
}
