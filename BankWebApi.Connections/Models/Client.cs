using System.ComponentModel.DataAnnotations;

namespace BankWebApi.Connections.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public required string Gender { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Income { get; set; }
        public ICollection<Account>? Accounts { get; set; }
    }
}
