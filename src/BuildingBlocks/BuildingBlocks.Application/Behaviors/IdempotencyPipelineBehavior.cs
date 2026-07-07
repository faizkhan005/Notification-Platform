using MediatR;
using System.Text.Json;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Intercepts commands implementing IIdempotentCommand, checks the store
/// for a previous result under the same key, and short-circuits with the
/// cached response if found — the handler never runs a second time.
///
/// PLACEMENT IN THE PIPELINE: this should run AFTER validation (no point
/// deduplicating a request that's malformed anyway) but BEFORE the handler.
/// Registration order in DependencyInjection.cs controls this.
/// </summary>
public sealed class IdempotencyPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIdempotencyStore _store;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    public IdempotencyPipelineBehavior(IIdempotencyStore store)
    {
        _store = store;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand idempotent)
            return await next();

        var cacheKey = $"idempotency:{idempotent.IdempotencyKey}";
        var cached = await _store.GetAsync(cacheKey, cancellationToken);

        if (cached is not null)
            return JsonSerializer.Deserialize<TResponse>(cached)!;

        var response = await next();

        var serialized = JsonSerializer.Serialize(response);
        await _store.SetAsync(cacheKey, serialized, DefaultTtl, cancellationToken);

        return response;
    }
}

