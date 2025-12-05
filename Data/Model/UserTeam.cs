using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

/// <summary>
/// Join table for many-to-many relationship between Users and Teams
/// </summary>
public class UserTeam
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
