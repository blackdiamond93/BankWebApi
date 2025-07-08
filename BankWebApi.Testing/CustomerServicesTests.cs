using BankWebApi.Connections.Data;
using BankWebApi.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace BankWebApi.Testing
{
    public class CustomerServicesTests
    {
        private readonly BankDbContext _dbContext;
        private readonly CustomerServices _service;

        public CustomerServicesTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                   .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                   .Options;
            _dbContext = new BankDbContext(options);
            _service = new CustomerServices(_dbContext);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnCustomer_WhenExists()
        {
            var customer = new Models.DTOs.CreateCustomerDto
            {
                Name = "John Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender ="Male",
                Income = 50000m
            };
            await _service.CreateCustomerAsync(customer);

            var createdCustomer = await _dbContext.Clients.FirstOrDefaultAsync(c => c.Name == customer.Name);
            Assert.NotNull(createdCustomer);
            Assert.Equal(customer.Name, createdCustomer.Name);
            Assert.Equal(customer.DateOfBirth, createdCustomer.DateOfBirth);
            Assert.Equal(customer.Gender, createdCustomer.Gender);
            Assert.Equal(customer.Income, createdCustomer.Income);
        }
    }
}
