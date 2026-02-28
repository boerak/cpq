namespace Cpq.Api.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public Guid EntityId { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }

    public ConcurrencyConflictException(Guid entityId, int expectedVersion, int actualVersion)
        : base($"Concurrency conflict on entity '{entityId}'. Expected version {expectedVersion} but found {actualVersion}.")
    {
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
