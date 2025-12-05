using System.ComponentModel.DataAnnotations;

namespace TeamCashCenter.Data.Model;

public class Account
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string Description { get; set; } = null!;
    
    [StringLength(50)]
    public string? AccountNumber { get; set; }

    [Required]
    [Range(typeof(decimal), "-1000000", "100000000")] 
    public decimal Balance { get; set; }
    
    [Required]
    [Range(typeof(decimal), "-1000000", "100000000")] 
    public decimal StartBalance { get; set; }

    public int Order { get; set; } = 1;

    public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
    
    public Account()
    {
    }
    
    public Account(string name, string description, decimal startBalance, int order = 9999) : this()
    {
        Order = order;
        Name = name;
        Description = description;
        Balance = startBalance;
        StartBalance = startBalance;
    }
    
    public Account(string name, string description, string accountNumber, decimal startBalance, int order = 9999) : this(name, description, startBalance, order)
    {
        AccountNumber = accountNumber;
    }
}
