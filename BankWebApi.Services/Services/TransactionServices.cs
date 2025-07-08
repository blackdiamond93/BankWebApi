using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models;
using BankWebApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankWebApi.Services.Services
{
    public class TransactionServices : ITransactionServices
    {
        private readonly BankDbContext _db;
        private readonly IAccountServices _accountService;
        public TransactionServices(BankDbContext db, IAccountServices accountService)
        {
            _db = db;
            _accountService = accountService;
        }

        public async Task<AccountSummaryDto> GetAccountSummaryByAccountNumberAsync(string accountNumber)
        {
            var historial = await GetHistoryByAccountAsync(accountNumber);

            var transactions = historial.Select(t=> new TransactionSummaryDto
            {
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                BalanceAfterTransaction = t.BalanceAfterTransaction

            }).ToList();

            var currentBalance = transactions
                .Where(t => t.TransactionType == TransactionType.Deposit.ToString())
                .Sum(t => t.Amount) - 
                transactions
                .Where(t => t.TransactionType == TransactionType.Withdrawal.ToString())
                .Sum(t => t.Amount);

            return new AccountSummaryDto
            {
                AccountNumber = accountNumber,
                CurrentBalance = currentBalance,
                Transactions = transactions
            };
        }

        public async Task<IEnumerable<Transaction>> GetHistoryByAccountAsync(string accountNumber)
        {
            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber)
                    ?? throw new KeyNotFoundException("Account not found");

            return await _db.Transactions
                .Where(t => t.AccountId == account.Id)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> RegisterTransactionAsync(string accountNumber, TransactionType type, decimal amount)
        {

            var account = await _db.Accounts
          .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber)
          ?? throw new KeyNotFoundException("Account not found");

            var currentBalance = await _accountService.GetBalanceByAccountNumberAsync(accountNumber);

            if (type == TransactionType.Withdrawal && amount > currentBalance)
                throw new InvalidOperationException("Insufficient funds");

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionType = type.ToString(),
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = type == TransactionType.Deposit
                                   ? currentBalance + amount
                                   : currentBalance - amount
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return transaction;
        
        }
    }
}
