namespace BankWebApi.Models.DTOs
{
    public class TransactionSummaryDto
    {
        public required string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
