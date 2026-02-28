namespace Cpq.Api.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, string field, object value)
        : base($"{entityType} with {field} '{value}' was not found.")
    {
        EntityType = entityType;
        EntityId = value;
    }
}
