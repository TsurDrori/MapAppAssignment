using MapServer.Domain.Exceptions;

namespace MapServer.Domain.ValueObjects;

/// <summary>
/// A closed ring of coordinates forming a valid polygon boundary.
/// Validates geometric invariants: closure, minimum points, no self-intersection.
/// </summary>
public class LinearRing
{
    public IReadOnlyList<(double Longitude, double Latitude)> Coordinates { get; }

    private LinearRing(IReadOnlyList<(double Longitude, double Latitude)> coordinates)
    {
        Coordinates = coordinates;
    }

    /// <summary>
    /// Creates a valid linear ring from coordinates.
    /// Auto-closes if needed and validates geometric rules.
    /// </summary>
    /// <exception cref="InvalidGeometryException">Thrown if geometry is invalid.</exception>
    public static LinearRing Create(IEnumerable<(double Longitude, double Latitude)> coordinates)
    {
        var coords = coordinates.ToList();

        if (coords.Count < 3)
        {
            throw new InvalidGeometryException("Polygon must have at least 3 coordinates");
        }

        var closed = EnsureClosed(coords);
        ValidateNoDuplicateConsecutivePoints(closed);
        ValidateNoSelfIntersection(closed);

        return new LinearRing(closed);
    }

    private static List<(double Longitude, double Latitude)> EnsureClosed(
        List<(double Longitude, double Latitude)> coordinates)
    {
        var first = coordinates[0];
        var last = coordinates[^1];

        if (first.Longitude == last.Longitude && first.Latitude == last.Latitude)
        {
            return coordinates;
        }

        return [.. coordinates, first];
    }

    private static void ValidateNoDuplicateConsecutivePoints(
        List<(double Longitude, double Latitude)> coordinates)
    {
        for (var i = 0; i < coordinates.Count - 1; i++)
        {
            if (PointsEqual(coordinates[i], coordinates[i + 1]))
            {
                throw new InvalidGeometryException($"Polygon contains a zero-length edge at index {i}");
            }
        }
    }

    private static void ValidateNoSelfIntersection(
        List<(double Longitude, double Latitude)> closedCoordinates)
    {
        var segmentCount = closedCoordinates.Count - 1;

        for (var i = 0; i < segmentCount; i++)
        {
            var a1 = closedCoordinates[i];
            var a2 = closedCoordinates[i + 1];

            for (var j = i + 1; j < segmentCount; j++)
            {
                if (AreAdjacentSegments(i, j, segmentCount))
                {
                    continue;
                }

                var b1 = closedCoordinates[j];
                var b2 = closedCoordinates[j + 1];

                if (SegmentsIntersect(a1, a2, b1, b2))
                {
                    throw new InvalidGeometryException("Polygon edges must not cross (self-intersection detected)");
                }
            }
        }
    }

    private static bool AreAdjacentSegments(int i, int j, int segmentCount)
    {
        if (j == i + 1) return true;
        return i == 0 && j == segmentCount - 1;
    }

    private static bool SegmentsIntersect(
        (double X, double Y) p1, (double X, double Y) q1,
        (double X, double Y) p2, (double X, double Y) q2)
    {
        var o1 = Orientation(p1, q1, p2);
        var o2 = Orientation(p1, q1, q2);
        var o3 = Orientation(p2, q2, p1);
        var o4 = Orientation(p2, q2, q1);

        if (o1 != o2 && o3 != o4)
            return true;

        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
        if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

        return false;
    }

    private static int Orientation(
        (double X, double Y) p, (double X, double Y) q, (double X, double Y) r)
    {
        const double epsilon = 1e-12;
        var cross = (q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X);

        if (Math.Abs(cross) <= epsilon) return 0;
        return cross > 0 ? 1 : -1;
    }

    private static bool OnSegment(
        (double X, double Y) p, (double X, double Y) q, (double X, double Y) r)
    {
        const double epsilon = 1e-12;
        return q.X <= Math.Max(p.X, r.X) + epsilon
            && q.X >= Math.Min(p.X, r.X) - epsilon
            && q.Y <= Math.Max(p.Y, r.Y) + epsilon
            && q.Y >= Math.Min(p.Y, r.Y) - epsilon;
    }

    private static bool PointsEqual((double X, double Y) a, (double X, double Y) b)
    {
        const double epsilon = 1e-12;
        return Math.Abs(a.X - b.X) <= epsilon && Math.Abs(a.Y - b.Y) <= epsilon;
    }
}
