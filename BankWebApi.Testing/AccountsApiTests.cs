using System.Net.Http.Json;
using System.Text.Json;

namespace BankWebApi.Testing
{
    public class AccountsApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public AccountsApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task CreateAccount_ShouldReturn201AndAccountData()
        {
            var customer = new { name = "Test User", dateOfBirth = "1990-01-01", gender = "Male", income = 10000 };
            var customerResp = await _client.PostAsJsonAsync("/api/customers", customer);
            customerResp.EnsureSuccessStatusCode();
            var customerJson = await customerResp.Content.ReadAsStringAsync();
            var customerObj = JsonSerializer.Deserialize<JsonElement>(customerJson);
            var customerId = customerObj.GetProperty("id").GetInt32();
            // Crear cuenta
            var account = new { accountNumber = "ACC01", clientId = customerId, balance = 1000.00m, createdAt = DateTime.UtcNow };
            var response = await _client.PostAsJsonAsync("/api/accounts", account);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("ACC01", json);
        }
    

        [Fact]
        public async Task CreateAccount_NonExistingCustomer_ShouldReturn404()
        {
            var account = new { accountNumber = "ACC02", clientId = 9999, balance = 1000.00m, createdAt = DateTime.UtcNow };
            var response = await _client.PostAsJsonAsync("/api/accounts", account);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

       [Fact]
       public async Task CreateAccount_DuplicateNumber_ShouldReturn409()
        {
            // Crear cliente primero
            var customer = new { name = "Test User1", dateOfBirth = "1990-01-01", gender = "Male", income = 10000 };
            var customerResp = await _client.PostAsJsonAsync("/api/customers", customer);
            customerResp.EnsureSuccessStatusCode();
            var customerJson = await customerResp.Content.ReadAsStringAsync();
            var customerObj = JsonSerializer.Deserialize<JsonElement>(customerJson);
            var customerId = customerObj.GetProperty("id").GetInt32();
            var account = new { accountNumber = "CA2034", clientId = customerId, balance = 1000.00m, createdAt = DateTime.UtcNow };
            // First attempt to create the account
            var response1 = await _client.PostAsJsonAsync("/api/accounts", account);
            response1.EnsureSuccessStatusCode();
            // Second attempt to create the same account should fail
            var response2 = await _client.PostAsJsonAsync("/api/accounts", account);
            Assert.Equal(System.Net.HttpStatusCode.Conflict, response2.StatusCode);
        }

        [Fact]
        public async Task CreateAccount_InvalidData_ShouldReturn400()
        {
            var account = new { customerId = 1, accountType = "", balance = -100 };
            var response = await _client.PostAsJsonAsync("/api/accounts", account);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetBalance_NonExistingAccount_ShouldReturn404()
        {
            var response = await _client.GetAsync("/api/accounts/balance/NOEXISTE");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetBalance_EmptyAccountNumber_ShouldReturn400()
        {
            var response = await _client.GetAsync("/api/accounts/balance/");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateAndGetBalance_ShouldReturnCorrectBalance()
        {
            // Crear cliente primero
            var uniqueName = $"Test User {Guid.NewGuid()}";
            var customer = new { Name = uniqueName, DateOfBirth = new DateTime(1990, 1, 1), Gender = "Male", Income = 10000m };
            var customerResp = await _client.PostAsJsonAsync("/api/customers", customer);
            customerResp.EnsureSuccessStatusCode();
            var customerJson = await customerResp.Content.ReadAsStringAsync();
            var customerObj = System.Text.Json.JsonDocument.Parse(customerJson).RootElement;
            var customerId = customerObj.GetProperty("id").GetInt32();
            // Crear cuenta
            var account = new { AccountNumber = "ACC100", ClientId = customerId, Balance = 500m, CreatedAt = DateTime.UtcNow };
            var accountResp = await _client.PostAsJsonAsync("/api/accounts", account);
            accountResp.EnsureSuccessStatusCode();
            var accountJson = await accountResp.Content.ReadAsStringAsync();
            var accountObj = System.Text.Json.JsonDocument.Parse(accountJson).RootElement;
            var accountNumber = accountObj.GetProperty("accountNumber").GetString();
            // Consultar saldo
            var balanceResp = await _client.GetAsync($"/api/accounts/balance/{accountNumber}");
            balanceResp.EnsureSuccessStatusCode();
            var balanceJson = await balanceResp.Content.ReadAsStringAsync();
            Assert.Contains("500", balanceJson);
        }
    }
}