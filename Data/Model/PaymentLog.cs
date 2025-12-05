namespace TeamCashCenter.Data.Model;

public class PaymentLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = null!; // e.g. "Payment", "Undo"
    public Guid? TransactionId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
}
