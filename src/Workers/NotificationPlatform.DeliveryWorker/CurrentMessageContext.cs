namespace NotificationPlatform.DeliveryWorker;

/// <summary>
/// Holds the correlation ID of the message currently being processed by
/// this consumer invocation. Registered as Scoped — MassTransit creates a
/// new DI scope per consumed message, so this naturally resets between messages.
///
/// The consumer sets this at the start of Consume(). Any scoped service
/// resolved later in the same scope (like UnitOfWork via ICorrelationIdProvider)
/// can read it without needing ConsumeContext threaded through every call.
/// </summary>
public sealed class CurrentMessageContext
{
    public string? CorrelationId { get; set; }
}