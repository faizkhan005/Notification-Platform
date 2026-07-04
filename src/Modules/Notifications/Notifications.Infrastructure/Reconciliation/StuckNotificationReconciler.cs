using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Reconciliation;

/// <summary>
/// Background service that detects notifications stuck in Processing status
/// for longer than the timeout — a sign the consumer crashed or the process
/// died mid-delivery without ever reaching MarkAsDelivered/MarkAsFailed.
///
/// This is distinct from the consumer's own try/catch: that only handles
/// exceptions the consumer code can observe. A hard process crash (kill -9,
/// OOM, forced restart) leaves NO code running to catch anything — the
/// notification is simply abandoned mid-flight. Only an external, periodic
/// check based on elapsed time can detect and recover from this.
/// </summary>
public sealed class StuckNotificationReconciler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StuckNotificationReconciler> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan StuckThreshold = TimeSpan.FromMinutes(5);

    public StuckNotificationReconciler(
        IServiceScopeFactory scopeFactory,
        ILogger<StuckNotificationReconciler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileStuckNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stuck notification reconciler encountered an unexpected error.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ReconcileStuckNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        var cutoff = DateTimeOffset.UtcNow - StuckThreshold;

        var stuckNotifications = await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Processing && n.UpdatedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (stuckNotifications.Count == 0) return;

        _logger.LogWarning(
            "Found {Count} notification(s) stuck in Processing for over {Threshold}. Marking as Failed.",
            stuckNotifications.Count, StuckThreshold);

        foreach (var notification in stuckNotifications)
        {
            notification.MarkAsFailed(
                $"Reconciler: notification was stuck in Processing for over {StuckThreshold.TotalMinutes} minutes " +
                "with no update — likely a worker crash or hung operation. Eligible for replay.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
