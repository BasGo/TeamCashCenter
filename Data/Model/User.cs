using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TeamCashCenter.Data.Model;

public class User : IdentityUser<Guid>
{
    // Extend with additional profile fields if needed
    
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Range(0, 99)]
    public int? JerseyNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [StringLength(50)]
    public string? PassportNumber { get; set; }

    public ICollection<Membership>? Memberships { get; set; }
    public ICollection<Payment>? Payments { get; set; }
    public ICollection<Transaction>? Transactions { get; set; }
    
    public bool HasKey =>  Id != Guid.Empty;
    
    public string DisplayName => FirstName + " " + LastName;
}
