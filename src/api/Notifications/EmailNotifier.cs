using CivicConnect.Requests;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

namespace CivicConnect.Notifications;

public sealed class EmailNotifier(IOptions<EmailOptions> options) : INotifier
{
    public static MimeMessage BuildMessage(StatusChangeNotification notification, EmailOptions settings)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(new MailboxAddress(notification.RecipientName, notification.RecipientEmail));
        message.Subject = $"CivicConnect: request #{notification.RequestId} is now {notification.NewStatus}";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Hello {notification.RecipientName},

                Your service request has moved to a new status.

                Request ID: {notification.RequestId}
                New status: {notification.NewStatus}

                This is an automated message. Replying to this email will not change your request.
                Sign in to CivicConnect to view it.
                """
        };

        return message;
    }

    public async Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var message = BuildMessage(notification, settings);

        try
        {
            using var client = new SmtpClient();
            client.Timeout = settings.TimeoutSeconds * 1000;

            var security = settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(settings.Host, settings.Port, security, cancellationToken);

            if (!string.IsNullOrEmpty(settings.Username))
            {
                await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Provider messages can echo the recipient's address, so only the exception type is kept.
            throw new NotificationDeliveryException($"SMTP delivery failed ({ex.GetType().Name}).", ex);
        }
    }
}