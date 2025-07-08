namespace BankWebApi.Models.DTOs
{
    public class AccountSummaryDto
    {
        public required string AccountNumber { get; set; }
        public IEnumerable<TransactionSummaryDto> Transactions { get; set; } = new List<TransactionSummaryDto>();
        public decimal CurrentBalance { get; set; }
    }
}
