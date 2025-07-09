using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Client?> GetCustomerByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Clients.AnyAsync(c => c.Id == customerId);
        }

        public async Task<bool> CustomerExistsByDataAsync(CreateCustomerDto dto)
        {
            return await _context.Clients.AnyAsync(c =>
                c.Name == dto.Name &&
                c.DateOfBirth == dto.DateOfBirth &&
                c.Gender == dto.Gender &&
                c.Income == dto.Income);
        }
    }
}
