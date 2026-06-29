namespace Notifications.Domain;

public readonly record struct NotificationId(Guid Value)
{
    public static NotificationId New() => new(Guid.NewGuid());
    public static NotificationId From(Guid value) => new(value);
    public override string ToString() => $"NotificationId({Value})";
}
