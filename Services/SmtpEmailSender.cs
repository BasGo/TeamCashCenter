using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TeamCashCenter.Services.Contracts;

namespace TeamCashCenter.Services;

public class SmtpEmailSender(IOptions<EmailOptions> opts) : IEmailSender
{
    private readonly EmailOptions _opts = opts.Value;

    public Task SendEmailAsync(string to, string subject, string htmlMessage, string? plainTextMessage = null)
    {
        var from = _opts.From ?? _opts.Username ?? "no-reply@example.com";

        var msg = new MailMessage();
        msg.From = new MailAddress(from);
        msg.To.Add(new MailAddress(to));
        msg.Subject = subject;
        msg.IsBodyHtml = true;

        // Add html body
        var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage ?? string.Empty, null, "text/html");
        msg.AlternateViews.Add(htmlView);

        // Prefer an explicit plain-text message if supplied, otherwise create a simple fallback by stripping tags
        var plain = !string.IsNullOrEmpty(plainTextMessage) ? plainTextMessage! : System.Text.RegularExpressions.Regex.Replace(htmlMessage ?? string.Empty, "<[^>]+>", string.Empty);
        var textView = AlternateView.CreateAlternateViewFromString(plain, null, "text/plain");
        msg.AlternateViews.Add(textView);

        var client = new SmtpClient(_opts.Host ?? "localhost", _opts.Port)
        {
            EnableSsl = _opts.EnableSsl
        };
        if (!string.IsNullOrEmpty(_opts.Username))
        {
            client.Credentials = new NetworkCredential(_opts.Username, _opts.Password ?? string.Empty);
        }

        return client.SendMailAsync(msg);
    }
}
