using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Team
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public ICollection<UserTeam>? UserTeams { get; set; }

    // Add more properties as needed (e.g., Description, CreatedAt)
}
