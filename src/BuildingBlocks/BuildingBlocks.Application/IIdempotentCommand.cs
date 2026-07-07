namespace BuildingBlocks.Application;

/// <summary>
/// Marks a command as idempotent — safe to deduplicate by key.
/// Commands implementing this interface get automatic deduplication
/// via IdempotencyPipelineBehavior, backed by Redis.
/// </summary>
public interface IIdempotentCommand
{
    string IdempotencyKey { get; }
}

