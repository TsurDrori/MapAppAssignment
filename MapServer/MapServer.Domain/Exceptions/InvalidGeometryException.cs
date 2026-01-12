namespace MapServer.Domain.Exceptions;

/// <summary>
/// Exception for invalid geographic geometry (e.g., self-intersecting polygons).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class InvalidGeometryException : ValidationException
{
    public InvalidGeometryException(string message) : base(message)
    {
    }

    public InvalidGeometryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
