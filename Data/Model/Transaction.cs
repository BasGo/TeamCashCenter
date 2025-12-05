using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Transaction
{
    public Guid Id { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }
    
    public Guid? ParentId { get; set; }
    public Transaction? Parent { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required]
    [Range(-10000000, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(150)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Team ID for multi-team support
    /// </summary>
    public Guid TeamId { get; set; }
    
    [Required]
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    public Guid Reference { get; set; }
    
    public IList<Transaction> SubTransactions { get; set; } = new List<Transaction>();
    
    public Transaction GetCopy()
    {
        return new Transaction
        {
            Id = Id,
            BookingDate = BookingDate,
            Amount = Amount,
            Description = Description,
            AccountId = AccountId,
            UserId = UserId,
            Reference =  Reference
        };
    }
}