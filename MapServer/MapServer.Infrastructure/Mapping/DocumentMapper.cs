using MapServer.Domain.Entities;
using MapServer.Infrastructure.Documents;

namespace MapServer.Infrastructure.Mapping;

/// <summary>
/// Maps between Domain entities and Infrastructure documents.
/// Domain entities use GeoJSON types (domain knowledge).
/// Documents add BSON attributes (persistence concern).
/// </summary>
public static class DocumentMapper
{
    #region Polygon Mapping

    public static PolygonDocument ToDocument(Polygon polygon)
    {
        return new PolygonDocument
        {
            Id = polygon.Id,
            Geometry = polygon.Geometry
        };
    }

    public static Polygon ToDomain(PolygonDocument document)
    {
        return new Polygon
        {
            Id = document.Id,
            Geometry = document.Geometry
        };
    }

    #endregion

    #region MapObject Mapping

    public static MapObjectDocument ToDocument(MapObject mapObject)
    {
        return new MapObjectDocument
        {
            Id = mapObject.Id,
            Location = mapObject.Location,
            ObjectType = mapObject.ObjectType
        };
    }

    public static MapObject ToDomain(MapObjectDocument document)
    {
        return new MapObject
        {
            Id = document.Id,
            Location = document.Location,
            ObjectType = document.ObjectType
        };
    }

    #endregion
}
