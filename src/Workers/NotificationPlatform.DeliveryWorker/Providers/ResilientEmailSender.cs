using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace NotificationPlatform.DeliveryWorker.Providers;

/// <summary>
/// Decorates IEmailSender with retry and circuit breaker resilience patterns.
///
/// This class does NOT know how to send an email — it only knows how to
/// retry and protect against cascading failures. The actual sending is
/// delegated to the wrapped IEmailSender (MailKitEmailSender).
///
/// PIPELINE ORDER (outermost to innermost):
///   Circuit Breaker → Retry → Actual Send
///
/// WHY this order specifically?
/// The circuit breaker wraps the retry, not the other way around.
/// If retry wrapped the circuit breaker, each retry attempt would
/// independently trip through the breaker's state, and a single logical
/// "send attempt" could flip the circuit open mid-retry in confusing ways.
/// With circuit breaker outermost: if the circuit is OPEN, we fail fast
/// WITHOUT even attempting the retries — that's the entire point of a
/// circuit breaker, avoiding wasted retry attempts against a known-down service.
/// </summary>
public sealed class ResilientEmailSender : IEmailSender
{
    private readonly IEmailSender _innerSender;
    private readonly ILogger<ResilientEmailSender> _logger;
    private readonly ResiliencePipeline _pipeline;

    public ResilientEmailSender(IEmailSender innerSender, ILogger<ResilientEmailSender> logger)
    {
        _innerSender = innerSender;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            // Circuit breaker — OUTERMOST layer.
            // Opens after 3 consecutive failures within the sampling window.
            // While open, calls fail IMMEDIATELY without attempting the network call.
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,               // trip on 100% failure rate within the window
                MinimumThroughput = 3,             // need at least 3 calls before evaluating
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15), // stay open for 15s before half-open test
                OnOpened = args =>
                {
                    _logger.LogWarning(
                        "Circuit breaker OPENED for email provider. " +
                        "Failing fast for {BreakDuration}s.", args.BreakDuration.TotalSeconds);
                    return default;
                },
                OnClosed = _ =>
                {
                    _logger.LogInformation("Circuit breaker CLOSED. Email provider recovered.");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    _logger.LogInformation("Circuit breaker HALF-OPEN. Testing email provider.");
                    return default;
                }
            })
            // Retry — INNER layer. Only reached if the circuit is closed (or half-open test).
            // Exponential backoff with jitter: 1s, 2s, 4s roughly, with randomness added
            // so multiple concurrent failures don't all retry at exactly the same moment
            // (which would cause a "thundering herd" hitting the recovering service at once).
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {AttemptNumber} for email send after failure: {Reason}",
                        args.AttemptNumber + 1, args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    public async Task SendAsync(
        string toAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(async ct =>
        {
            await _innerSender.SendAsync(toAddress, subject, body, ct);
        }, cancellationToken);
    }
}
