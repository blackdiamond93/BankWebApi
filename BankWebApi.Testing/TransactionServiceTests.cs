using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models;
using BankWebApi.Services.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankWebApi.Testing
{
    public class TransactionServiceTests
    {
        private readonly BankDbContext _dbContext;
        private readonly Mock<IAccountServices> _accountServiceMock;
        private readonly TransactionServices _service;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new BankDbContext(options);
            _accountServiceMock = new Mock<IAccountServices>();
            _service = new TransactionServices(_dbContext, _accountServiceMock.Object);
        }

        [Fact]
        public async Task RegisterTransactionAsync_Deposit_ShouldAddTransactionWithCorrectBalance()
        {
            var account = new Account { Id = 1, AccountNumber = "ACC123" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            _accountServiceMock
                .Setup(s => s.GetBalanceByAccountNumberAsync("ACC123"))
                .ReturnsAsync(100m);
            var result = await _service.RegisterTransactionAsync("ACC123", TransactionType.Deposit, 50m);
            Assert.NotNull(result);
            Assert.Equal(account.Id, result.AccountId);
            Assert.Equal("Deposit", result.TransactionType);
            Assert.Equal(50m, result.Amount);
            Assert.Equal(150m, result.BalanceAfterTransaction);
            var stored = await _dbContext.Transactions.FirstOrDefaultAsync();
            Assert.NotNull(stored);
        }

        [Fact]
        public async Task RegisterTransactionAsync_Withdrawal_WithInsufficientFunds_ShouldThrowInvalidOperationException()
        {
            var account = new Account { Id = 1, AccountNumber = "ACC123" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            _accountServiceMock
                .Setup(s => s.GetBalanceByAccountNumberAsync("ACC123"))
                .ReturnsAsync(30m);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterTransactionAsync("ACC123", TransactionType.Withdrawal, 50m));
        }

        [Fact]
        public async Task RegisterTransactionAsync_Withdrawal_WithSufficientFunds_ShouldSubtractAmount()
        {
            var account = new Account { Id = 1, AccountNumber = "ACC123" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            _accountServiceMock
                .Setup(s => s.GetBalanceByAccountNumberAsync("ACC123"))
                .ReturnsAsync(200m);
            var result = await _service.RegisterTransactionAsync("ACC123", TransactionType.Withdrawal, 50m);
            Assert.Equal("Withdrawal", result.TransactionType);
            Assert.Equal(150m, result.BalanceAfterTransaction);
        }

        [Fact]
        public async Task GetHistoryByAccountAsync_WithExistingTransactions_ReturnsOrderedList()
        {
            var account = new Account { AccountNumber = "ACC1" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            _dbContext.Transactions.AddRange(
                new Transaction { AccountId = account.Id,TransactionType= "Deposit", Amount = 10m, TransactionDate = new DateTime(2025, 7, 1) },
                new Transaction { AccountId = account.Id, TransactionType = "Withdrawal", Amount = 20m, TransactionDate = new DateTime(2025, 7, 3) },
                new Transaction { AccountId = account.Id, TransactionType = "Withdrawal", Amount = 15m, TransactionDate = new DateTime(2025, 7, 2) }
            );
            await _dbContext.SaveChangesAsync();

            var history = await _service.GetHistoryByAccountAsync("ACC1");

            Assert.Equal(3, history.Count());
            Assert.Collection(history,
                t => Assert.Equal(new DateTime(2025, 7, 1), t.TransactionDate),
                t => Assert.Equal(new DateTime(2025, 7, 2), t.TransactionDate),
                t => Assert.Equal(new DateTime(2025, 7, 3), t.TransactionDate)
            );
        }

        [Fact]
        public async Task GetHistoryByAccountAsync_NoTransactions_ReturnsEmptyList()
        {
            var account = new Account { AccountNumber = "ACC2" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            var history = await _service.GetHistoryByAccountAsync("ACC2");
            Assert.Empty(history);
        }

        [Fact]
        public async Task GetHistoryByAccountAsync_AccountNotFound_ThrowsKeyNotFoundException()
        {  
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetHistoryByAccountAsync("NON_EXISTENT"));
        }

        [Fact]
        public async Task GetAccountSummaryByAccountNumberAsync_ReturnsCorrectSummary()
        {
            
            var account = new Account { AccountNumber = "ACC3", Balance =100m, ClientId =1, CreatedAt = DateTime.UtcNow };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();


            _dbContext.Transactions.AddRange(
                new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Deposit",
                    Amount = 100m,
                    TransactionDate = new DateTime(2025, 7, 1),
                    BalanceAfterTransaction = 100m
                },
                new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Withdrawal",
                    Amount = 40m,
                    TransactionDate = new DateTime(2025, 7, 2),
                    BalanceAfterTransaction = 40m
                }
            );
            await _dbContext.SaveChangesAsync();

            var expectedBalance = 60m;
            var expectedHistoryCount = 2;


            var result = await _service.GetAccountSummaryByAccountNumberAsync(account.AccountNumber);
            Assert.Equal(account.AccountNumber, result.AccountNumber);
            Assert.Equal(expectedBalance, result.CurrentBalance);
            Assert.Equal(expectedHistoryCount, result.Transactions.Count());

            var resultados = result.Transactions.ToList();
            Assert.Equal("Deposit", resultados[0].TransactionType);
            Assert.Equal(100m, resultados[0].Amount);
            Assert.Equal(new DateTime(2025, 7, 1), resultados[0].TransactionDate);

            Assert.Equal("Withdrawal", resultados[1].TransactionType);
            Assert.Equal(40m, resultados[1].Amount);
            Assert.Equal(new DateTime(2025, 7, 2), resultados[1].TransactionDate);
        }

        [Fact]
        public async Task GetAccountSummaryByAccountNumberAsync_WithNoHistory_ReturnsEmptyTransactions()
        {

            var account = new Account { AccountNumber = "ACC2" };
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            var accountNumber = "ACC2";
            var result = await _service.GetAccountSummaryByAccountNumberAsync(accountNumber);

            Assert.Empty(result.Transactions);
        }
    }

}
