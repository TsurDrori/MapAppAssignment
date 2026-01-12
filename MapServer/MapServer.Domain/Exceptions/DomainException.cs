namespace MapServer.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level errors.
/// These represent business rule violations or invalid operations.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
