namespace TeamCashCenter.Services;

public class EmailOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }
    public string? AdminContact { get; set; }
    public string? DisplayName { get; set; }
    public int ResetTokenExpiryHours { get; set; } = 24;
}
