using System.ComponentModel.DataAnnotations;

namespace BankWebApi.Models.DTOs
{
    public class CreateCustomerDto
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public required string Gender { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Income { get; set; }
    }
}
