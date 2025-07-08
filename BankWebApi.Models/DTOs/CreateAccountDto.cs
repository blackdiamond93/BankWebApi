namespace BankWebApi.Models.DTOs
{
    public class CreateAccountDto
    {
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public int ClientId { get; set; } // Foreign key to Client
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
