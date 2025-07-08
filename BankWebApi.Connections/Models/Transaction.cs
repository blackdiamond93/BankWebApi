using System.ComponentModel.DataAnnotations;

namespace BankWebApi.Connections.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public required string TransactionType { get; set; } 
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
        public int AccountId { get; set; }
        public  Account? Account { get; set; }

    }
}
