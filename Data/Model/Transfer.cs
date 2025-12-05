using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Transfer
{
    public Guid Id { get; set; }

    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required]
    [Range(-10000000, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(150)]
    public string Description { get; set; } = null!;
    
    [Required]
    public Guid TargetAccountId { get; set; }
    public Account TargetAccount { get; set; } = null!;
    
    [Required]
    public Guid SourceAccountId { get; set; }
    public Account SourceAccount { get; set; } = null!;
    
    public Transfer GetCopy()
    {
        return new Transfer
        {
            Id = Id,
            BookingDate = BookingDate,
            Amount = Amount,
            Description = Description,
            SourceAccountId = SourceAccountId,
            TargetAccountId = TargetAccountId,
        };
    }
    
    public Transaction CreateSourceTransaction()
    {
        return new Transaction
        {
            Id = Guid.Empty,
            BookingDate = BookingDate,
            Amount = Amount * -1,
            Description = Description,
            AccountId = SourceAccountId,
            Reference = Id
        };
    }
    
    public Transaction CreateTargetTransaction()
    {
        return new Transaction
        {
            Id = Guid.Empty,
            BookingDate = BookingDate,
            Amount = Amount,
            Description = Description,
            AccountId = TargetAccountId,
            Reference = Id
        };
    }
}
