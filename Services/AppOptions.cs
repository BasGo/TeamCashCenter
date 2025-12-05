namespace TeamCashCenter.Services;

public class AppOptions
{
    public string? DisplayName { get; set; }
    
    public string? Claim { get; set; }

    public string? AdminEmail { get; set; } = "admin@verein.local";
    public string? AdminPassword { get; set; } = "Admin123!";
    public string? LocalDomain { get; set; } = "verein.local";
}
