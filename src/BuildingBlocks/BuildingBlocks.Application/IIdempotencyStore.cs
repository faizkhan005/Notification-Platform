namespace BuildingBlocks.Application;

/// <summary>
/// Abstracts idempotency key storage — Redis-backed in Infrastructure,
/// but Application never knows that. Same Dependency Inversion pattern
/// used for IUnitOfWork, ITenantRepository, etc.
/// </summary>
public interface IIdempotencyStore
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default);
}
