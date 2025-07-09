using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace BankWebApi.Testing
{
    public class TransactionApiTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public TransactionApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Withdraw_WithoutFunds_ShouldReturn422()
        {
            var withdrawal = new
            {
                accountNumber = "ACC3001",
                amount = 100m,
                transactionType = "Withdrawal"
            };

            var response = await _client.PostAsJsonAsync("/api/transactions", withdrawal);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Deposit_Successful_ShouldIncreaseBalance()
        {
            var deposit = new
            {
                accountNumber = "ACC3001",
                amount = 50m,
                transactionType = "Deposit"
            };

            var response = await _client.PostAsJsonAsync("/api/transactions", deposit);
            response.EnsureSuccessStatusCode();

            // Now check balance
            var balanceResponse = await _client.GetAsync($"/api/accounts/ACC3001/balance");
            balanceResponse.EnsureSuccessStatusCode();
            var balanceJson = await balanceResponse.Content.ReadAsStringAsync();
            Assert.Contains("100", balanceJson); // Adjust based on expected value
        }

        [Fact]
        public async Task GetTransactionHistory_ShouldReturnAllTransactions()
        {
            var response = await _client.GetAsync("/api/accounts/ACC3001/history");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Deposit", json);
            Assert.Contains("Withdrawal", json);
        }

    }
}
