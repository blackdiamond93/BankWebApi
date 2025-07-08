using System.ComponentModel.DataAnnotations;

namespace BankWebApi.Connections.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        public string AccountNumber { get; set; } = default!;
        [Required]
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }

    }
}
