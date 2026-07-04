using Notifications.Application;

namespace NotificationPlatform.DeliveryWorker;

/// <summary>
/// The Worker doesn't originate correlation IDs from an HTTP context — it
/// only consumes messages that already carry one from the API. This provider
/// is used only if UnitOfWork.SaveChangesAsync is called from the Worker's
/// own code paths (it isn't right now, since the Worker doesn't send commands,
/// but the dependency must still resolve for DI validation to pass).
/// </summary>
public sealed class ConsumeContextCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly CurrentMessageContext _currentMessageContext;

    public ConsumeContextCorrelationIdProvider(CurrentMessageContext currentMessageContext)
    {
        _currentMessageContext = currentMessageContext;
    }

    public string? GetCorrelationId() => _currentMessageContext.CorrelationId;
}
