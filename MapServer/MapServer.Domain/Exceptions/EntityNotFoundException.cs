namespace MapServer.Domain.Exceptions;

/// <summary>
/// Exception when a requested entity does not exist.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityType, string entityId)
        : base($"{entityType} with ID '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
