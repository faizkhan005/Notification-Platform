namespace Notifications.Domain;

public enum NotificationStatus
{
    Pending = 0,
    Processing = 1,
    Delivered = 2,
    Failed = 3,
    Retrying = 4
}
