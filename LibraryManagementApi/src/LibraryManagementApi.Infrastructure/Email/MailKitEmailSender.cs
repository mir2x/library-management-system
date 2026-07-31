using LibraryManagementApi.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LibraryManagementApi.Infrastructure.Email;

public class MailKitEmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
