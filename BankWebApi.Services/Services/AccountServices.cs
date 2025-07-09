using BankWebApi.Connections.Data;
using BankWebApi.Connections.Models;
using BankWebApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankWebApi.Services.Services
{
    public class AccountServices : IAccountServices
    {
        private readonly BankDbContext _db;
        public AccountServices(BankDbContext db) =>_db = db;

        public async Task<Account> ApplyInterestAsync(string accountNumber, decimal annualRate)
        {
            var account = await _db.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber) ?? throw new KeyNotFoundException("Cuenta no encontrada");

            var balance = await GetBalanceByAccountNumberAsync(accountNumber);
            var interest = balance * annualRate / 100m;
            account.Balance += interest;
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync();
            return account;
        }

        public async Task<Account> CreateAccountAsync(CreateAccountDto dto)
        {
            var account = new Account
            {
                AccountNumber = dto.AccountNumber,               
                Balance = dto.Balance,
                ClientId = dto.ClientId,
                CreatedAt = dto.CreatedAt
            };
            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();
            return account;
        }

        public async Task<decimal> GetBalanceByAccountNumberAsync(string accountNumber)
        {

            var cuenta = await _db.Accounts
                       .Include(c => c.Transactions)
                       .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber)
                       ?? throw new KeyNotFoundException("Cuenta no encontrada");
            return cuenta.Balance;

        }


    }
}
