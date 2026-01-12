namespace MapServer.Domain.Exceptions;

/// <summary>
/// Exception for validation errors (invalid input data).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
