namespace TeamCashCenter.Models
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime Date { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal Amount { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountName { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
