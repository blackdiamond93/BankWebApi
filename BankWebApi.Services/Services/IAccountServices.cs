using BankWebApi.Connections.Models;
using BankWebApi.Models.DTOs;

namespace BankWebApi.Services.Services
{
    public interface IAccountServices
    {
        Task<Account> CreateAccountAsync(CreateAccountDto dto);
        Task<decimal> GetBalanceByAccountNumberAsync(string accountNumber);

        Task<Account> ApplyInterestAsync(string accountNumber, decimal annualRate);
    }
}
