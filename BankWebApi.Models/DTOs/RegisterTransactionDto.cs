namespace BankWebApi.Models.DTOs
{
    public class RegisterTransactionDto
    {
        public required string AccountNumber { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
