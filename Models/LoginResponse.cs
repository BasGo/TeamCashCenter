namespace TeamCashCenter.Models;

public class LoginResponse
{
    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
    public string? ErrorMessage { get; set; }
}
