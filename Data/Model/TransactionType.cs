using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class TransactionType
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public bool IsIncome { get; set; }
    
    public bool IsRegularFee { get; set; }
}
