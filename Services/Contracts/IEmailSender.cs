namespace TeamCashCenter.Services.Contracts;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlMessage, string? plainTextMessage = null);
}
