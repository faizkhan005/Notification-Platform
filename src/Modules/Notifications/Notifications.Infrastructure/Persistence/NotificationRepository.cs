using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(
        NotificationId id,
        CancellationToken cancellationToken = default)
        => await _context.Notifications.FindAsync([id], cancellationToken);

    public async Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
        => await _context.Notifications.AddAsync(notification, cancellationToken);
}
