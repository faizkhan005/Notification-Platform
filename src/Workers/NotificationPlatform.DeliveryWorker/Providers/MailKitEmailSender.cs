using MimeKit;
using MailKit.Net.Smtp;

namespace NotificationPlatform.DeliveryWorker.Providers;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IConfiguration configuration, ILogger<MailKitEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(
        string toAddress,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(
            _configuration["Email:FromAddress"] ?? "noreply@notificationplatform.local"));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        var host = _configuration["Email:SmtpHost"] ?? "localhost";
        var port = int.Parse(_configuration["Email:SmtpPort"] ?? "1025");

        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Recipient} with subject {Subject}", toAddress, subject);
    }
}
