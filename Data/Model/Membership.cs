using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Membership
{
    public Guid Id { get; set; }

    [Required]
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    [Required]
    [Range(0, 10000)]
    public decimal MonthlyFee { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public bool IsActive => EndDate > DateTime.Now;
}
