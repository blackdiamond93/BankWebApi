using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models.DTOs;

namespace BankWebApi.Services.Services
{
    public class CustomerServices : ICustomerServices
    {
        private readonly BankDbContext _context;
        public CustomerServices(BankDbContext context) => _context = context;

        public async Task<Client> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var customer = new Client
            {
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Income = dto.Income
            };

            _context.Clients.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }
    }
}
