using BuildingBlocks.Application;
using StackExchange.Redis;

namespace Notifications.Infrastructure.Idempotency;

/// <summary>
/// Redis-backed implementation of IIdempotencyStore.
///
/// WHY Redis and not Postgres for this?
/// Idempotency keys are checked on EVERY request to an idempotent endpoint —
/// a high-frequency, low-latency read/write pattern. Redis's in-memory
/// storage and native TTL support (keys expire automatically, no cleanup
/// job needed) make it purpose-built for this. Postgres would work but
/// requires a manual expiry/cleanup job and adds latency to every request.
/// </summary>
public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisIdempotencyStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, ttl);
    }
}
