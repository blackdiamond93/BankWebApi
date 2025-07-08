using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models;
using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankWebApi.Testing
{
    public class AccountServicesTests
    {
        private readonly BankDbContext _dbContext;
        private readonly AccountServices _service;
        public AccountServicesTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                   .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                   .Options;
            _dbContext = new BankDbContext(options);
            _service = new AccountServices(_dbContext);
        }


        [Fact]
        public async Task CreateAccountAsync_ShouldAddAccountToDatabase()
        {

            var dto = new CreateAccountDto
            {
                AccountNumber = "ACC100",
                Balance = 200m,
                ClientId = 42,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.CreateAccountAsync(dto);
            Assert.NotNull(result);
            Assert.Equal(dto.AccountNumber, result.AccountNumber);
            Assert.Equal(dto.Balance, result.Balance);
            Assert.Equal(dto.ClientId, result.ClientId);
            var accountInDb = await _dbContext.Accounts.FindAsync(result.Id);
            Assert.NotNull(accountInDb);
        }

        [Fact]
        public async Task GetBalanceByAccountNumberAsync_NoTransactions_ReturnsInitialBalance()
        {
            var initialBalance = 150m;
            _dbContext.Accounts.Add(new Account
            {
                AccountNumber = "ACC101",
                Balance = initialBalance,
                ClientId = 1,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
            var balance = await _service.GetBalanceByAccountNumberAsync("ACC101");
            Assert.Equal(initialBalance, balance);
        }

        [Fact]
        public async Task GetBalanceByAccountNumberAsync_WithTransactions_CalculatesCorrectBalance()
        {
            var initialBalance = 100m;
            var acc = new Account
            {
                AccountNumber = "ACC102",
                Balance = initialBalance,
                ClientId = 2,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Accounts.Add(acc);
            await _dbContext.SaveChangesAsync();

            _dbContext.Transactions.AddRange(
                new Transaction { AccountId = acc.Id, TransactionType = "Deposit", Amount = 50m },
                new Transaction { AccountId = acc.Id, TransactionType = "Withdrawal", Amount = 20m },
                new Transaction { AccountId = acc.Id, TransactionType = "Deposit", Amount = 30m }
            );
            await _dbContext.SaveChangesAsync();
            ;
            var balance = await _service.GetBalanceByAccountNumberAsync("ACC102");
            var expected = initialBalance + 50m + 30m - 20m;
            Assert.Equal(expected, balance);
        }

        [Fact]
        public async Task GetBalanceByAccountNumberAsync_AccountNotFound_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetBalanceByAccountNumberAsync("NON_EXISTENT"));
        }

        [Fact]
        public async Task ApplyInterestAsync_ShouldIncreaseBalanceByCorrectAmount()
        {
            var account = new Account
            {
                AccountNumber = "ACC200",
                Balance = 1000m,
                ClientId = 1,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            var updated = await _service.ApplyInterestAsync("ACC200", annualRate: 5m);
            Assert.Equal(1050m, updated.Balance);
        }


    }
}