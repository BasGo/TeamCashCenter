using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class MailKitEmailSender(IOptions<EmailOptions> opts) : IEmailSender
{
    private readonly EmailOptions _opts = opts.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlMessage, string? plainTextMessage = null)
    {
        var message = new MimeMessage();
        var from = _opts.From ?? _opts.Username ?? "no-reply@example.com";
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = htmlMessage ?? string.Empty;
        builder.TextBody = !string.IsNullOrEmpty(plainTextMessage) ? plainTextMessage : StripTags(htmlMessage ?? string.Empty);
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        if (!string.IsNullOrEmpty(_opts.Host))
        {
            await client.ConnectAsync(_opts.Host, _opts.Port, _opts.EnableSsl).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(_opts.Username))
            {
                await client.AuthenticateAsync(_opts.Username, _opts.Password ?? string.Empty).ConfigureAwait(false);
            }
        }
        await client.SendAsync(message).ConfigureAwait(false);
        if (client.IsConnected)
        {
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }
    }

    private static string StripTags(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "<[^>]+>", string.Empty);
    }
}
