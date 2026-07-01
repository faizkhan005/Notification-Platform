namespace NotificationPlatform.DeliveryWorker.Providers;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}