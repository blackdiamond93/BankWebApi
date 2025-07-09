using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace BankWebApi.Testing
{
    public class TransactionApiTests: IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public TransactionApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<(int customerId, string accountNumber)> CreateCustomerAndAccount(decimal initialBalance = 50m)
        {
            var customer = new { Name = "Test User", DateOfBirth = new DateTime(1990, 1, 1), Gender = "Male", Income = 10000m };
            var customerResp = await _client.PostAsJsonAsync("/api/customers", customer);
            customerResp.EnsureSuccessStatusCode();
            var customerJson = await customerResp.Content.ReadAsStringAsync();
            var customerObj = System.Text.Json.JsonDocument.Parse(customerJson).RootElement;
            var customerId = customerObj.GetProperty("id").GetInt32();
            var accountNumber = $"ACC{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var account = new { AccountNumber = accountNumber, ClientId = customerId, Balance = initialBalance, CreatedAt = DateTime.UtcNow };
            var accountResp = await _client.PostAsJsonAsync("/api/accounts", account);
            accountResp.EnsureSuccessStatusCode();
            return (customerId, accountNumber);
        }

        [Fact]
        public async Task Withdraw_WithoutFunds_ShouldReturn422()
        {
            var (_, accountNumber) = await CreateCustomerAndAccount();
            var withdrawal = new
            {
                AccountNumber = accountNumber,
                TransactionType = 1, // Withdrawal
                Amount = 100m
            };
            var response = await _client.PostAsJsonAsync("/api/transactions/register", withdrawal);
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Deposit_Successful_ShouldIncreaseBalance()
        {
            var (_, accountNumber) = await CreateCustomerAndAccount();
            var deposit = new
            {
                AccountNumber = accountNumber,
                TransactionType = 0, // Deposit
                Amount = 50m
            };
            var response = await _client.PostAsJsonAsync("/api/transactions/register", deposit);
            response.EnsureSuccessStatusCode();
            // Now check balance
            var balanceResponse = await _client.GetAsync($"/api/accounts/balance/{accountNumber}");
            balanceResponse.EnsureSuccessStatusCode();
            var balanceJson = await balanceResponse.Content.ReadAsStringAsync();
            Assert.Contains("100", balanceJson); // 50 inicial + 50 depósito
        }

        [Fact]
        public async Task GetTransactionHistory_ShouldReturnAllTransactions()
        {
            var (_, accountNumber) = await CreateCustomerAndAccount();
            // Registrar un depósito y un retiro
            var deposit = new { AccountNumber = accountNumber, TransactionType = 0, Amount = 50m }; // Deposit
            await _client.PostAsJsonAsync("/api/transactions/register", deposit);
            var withdrawal = new { AccountNumber = accountNumber, TransactionType = 1, Amount = 20m }; // Withdrawal
            await _client.PostAsJsonAsync("/api/transactions/register", withdrawal);
            var response = await _client.GetAsync($"/api/transactions/history/{accountNumber}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Deposit", json);
            Assert.Contains("Withdrawal", json);
        }

    }
}
