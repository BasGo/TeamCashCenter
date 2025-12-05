namespace TeamCashCenter.Models;

public class RegisterResponse
{
    public bool Succeeded { get; set; }
    public List<string>? Errors { get; set; }
}
