using BankWebApi.Connections.Models;
using BankWebApi.Models;
using BankWebApi.Models.DTOs;

namespace BankWebApi.Services.Services
{
    public interface ITransactionServices
    {
        Task<Transaction> RegisterTransactionAsync(string accountNumber, TransactionType type, decimal amount);
        Task<IEnumerable<Transaction>> GetHistoryByAccountAsync(string accountNumber);
        Task<AccountSummaryDto> GetAccountSummaryByAccountNumberAsync(string accountNumber);
    }
}
