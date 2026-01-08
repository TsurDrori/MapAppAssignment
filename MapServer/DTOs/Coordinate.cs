// ============================================================================
// Coordinate.cs - A SIMPLE LATITUDE/LONGITUDE POINT
// ============================================================================
//
// WHAT: A simple container for a geographic coordinate (a point on Earth).
//
// WHY:  We need a simple way to represent locations. This is used:
//       - In API requests (client sends coordinates)
//       - In API responses (server sends coordinates back)
//       - As building blocks for polygons (a polygon is a list of coordinates)
//
// WHY NOT USE MONGODB'S GEOJSON TYPES?
//       The client (React app) doesn't know about MongoDB. Using a simple
//       class keeps the API clean and database-agnostic. The conversion to
//       MongoDB format happens in the Service layer.
//
// See BACKEND_CONCEPTS.md: DTOs (Data Transfer Objects), Classes and Objects
// ============================================================================

// This class lives in the MapServer.DTOs namespace
// "DTOs" = Data Transfer Objects = objects designed for sending/receiving data
namespace MapServer.DTOs;

// ============================================================================
// Coordinate CLASS
// ============================================================================
// This is a DTO - a simple data container with no behavior.
// Just holds data, that's it.
//
// "public class" = accessible from anywhere, is a blueprint for creating objects
// ============================================================================
public class Coordinate
{
    // ========================================================================
    // Latitude - North/South position
    // ========================================================================
    // Range: -90 (South Pole) to +90 (North Pole)
    // The equator is 0.
    //
    // "public" = accessible from outside this class
    // "double" = a decimal number (like 32.5429)
    // "{ get; set; }" = can be read and written
    //
    // WHY "double"?
    // - "int" would lose precision (no decimals)
    // - "decimal" is for money (overkill and slower)
    // - "double" is perfect for geographic coordinates
    //
    // See BACKEND_CONCEPTS.md: Properties (get/set)
    // ========================================================================
    public double Latitude { get; set; }

    // ========================================================================
    // Longitude - East/West position
    // ========================================================================
    // Range: -180 to +180
    // The Prime Meridian (through London) is 0.
    // Positive = East, Negative = West
    //
    // NOTE: In GeoJSON (the standard), coordinates are [longitude, latitude].
    // But many mapping libraries (like Leaflet) use [latitude, longitude].
    // Our React app handles this conversion.
    // ========================================================================
    public double Longitude { get; set; }
}
