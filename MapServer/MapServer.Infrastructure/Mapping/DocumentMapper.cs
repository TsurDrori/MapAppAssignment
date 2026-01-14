using MapServer.Domain.Entities;
using MapServer.Domain.ValueObjects;
using MapServer.Infrastructure.Documents;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MapServer.Infrastructure.Mapping;

/// <summary>
/// Maps between Domain entities and Infrastructure documents.
/// Handles GeoJSON conversion - this is the persistence boundary.
/// </summary>
public static class DocumentMapper
{
    #region Polygon Mapping

    public static PolygonDocument ToDocument(Polygon polygon)
    {
        var geometry = CreateGeoJsonPolygon(polygon.Coordinates);

        return new PolygonDocument
        {
            Id = polygon.Id,
            Geometry = geometry
        };
    }

    public static Polygon ToDomain(PolygonDocument document)
    {
        var coordinates = document.Geometry.Coordinates.Exterior.Positions
            .Select(pos => new GeoCoordinate(pos.Latitude, pos.Longitude))
            .ToList();

        return new Polygon
        {
            Id = document.Id,
            Coordinates = coordinates
        };
    }

    private static GeoJsonPolygon<GeoJson2DGeographicCoordinates> CreateGeoJsonPolygon(
        List<GeoCoordinate> coordinates)
    {
        var geoJsonCoordinates = coordinates
            .Select(c => new GeoJson2DGeographicCoordinates(c.Longitude, c.Latitude))
            .ToList();

        var linearRing = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(geoJsonCoordinates);

        return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRing)
        );
    }

    #endregion

    #region MapObject Mapping

    public static MapObjectDocument ToDocument(MapObject mapObject)
    {
        var location = CreateGeoJsonPoint(mapObject.Location);

        return new MapObjectDocument
        {
            Id = mapObject.Id,
            Location = location,
            ObjectType = mapObject.ObjectType
        };
    }

    public static MapObject ToDomain(MapObjectDocument document)
    {
        var position = document.Location.Coordinates;

        return new MapObject
        {
            Id = document.Id,
            Location = new GeoCoordinate(position.Latitude, position.Longitude),
            ObjectType = document.ObjectType
        };
    }

    private static GeoJsonPoint<GeoJson2DGeographicCoordinates> CreateGeoJsonPoint(GeoCoordinate coordinate)
    {
        return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(coordinate.Longitude, coordinate.Latitude)
        );
    }

    #endregion
}
