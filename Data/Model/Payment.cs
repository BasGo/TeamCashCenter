using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Payment
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? MembershipId { get; set; }
    public Membership? Membership { get; set; }
    
    public bool IsPaid { get; set; }

    public int? TransactionTypeId { get; set; }
    public TransactionType? TransactionType { get; set; }
    
    public ICollection<Transaction>? Transactions { get; set; }
    
    /// <summary>
    /// Team ID for multi-team support
    /// </summary>
    public Guid TeamId { get; set; }
    
    public List<Payment> CreateNewForUsers(IEnumerable<Guid> userIds)
    {
        return userIds.Select(u => new Payment
        {
            Id = Guid.Empty,
            Amount = this.Amount,
            DueDate = this.DueDate,
            Description = this.Description,
            MembershipId = this.MembershipId,
            TransactionTypeId = this.TransactionTypeId,
            IsPaid = this.IsPaid,

            UserId = u
        }).ToList();
    }
}
