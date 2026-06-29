using Notifications.Domain.Exceptions;

namespace Notifications.Domain;

public sealed record NotificationContent
{
    public string Subject { get; }
    public string Body { get; }

    private NotificationContent(string subject, string body)
    {
        Subject = subject;
        Body = body;
    }

    public static NotificationContent Create(string? subject, string? body)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new NotificationDomainException("Notification subject cannot be empty.");

        if (subject.Length > 500)
            throw new NotificationDomainException("Subject cannot exceed 500 characters.");

        if (string.IsNullOrWhiteSpace(body))
            throw new NotificationDomainException("Notification body cannot be empty.");

        return new NotificationContent(subject.Trim(), body.Trim());
    }
}
