using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BankWebApi.Testing
{
    public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CustomerApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateCustomer_ShouldReturn201AndCustomerData()
        {
            var customer = new { name = "Nuevo Cliente", dateOfBirth = "1985-05-05", gender = "Female", income = 20000 };
            var response = await _client.PostAsJsonAsync("/api/customers", customer);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nuevo Cliente", json);
        }

        [Fact]
        public async Task CreateCustomer_InvalidData_ShouldReturn400()
        {
            var customer = new
            {
                name = "",
                dateOfBirth = "2020-01-01",
                gender = "Alien",
                income = -5
            };

            var response = await _client.PostAsJsonAsync("/api/customers", customer);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCustomerById_ShouldReturnCustomer()
        {
            
            var customer = new { name = "Cliente Consulta", dateOfBirth = "1970-01-01", gender = "Male", income = 15000 };
            var createResp = await _client.PostAsJsonAsync("/api/customers", customer);
            createResp.EnsureSuccessStatusCode();
            var createJson = await createResp.Content.ReadAsStringAsync();
            var createObj = JsonSerializer.Deserialize<JsonElement>(createJson);
            var customerId = createObj.GetProperty("id").GetInt32();
            var response = await _client.GetAsync($"/api/customers/{customerId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Cliente Consulta", json);
        }

        [Fact]
        public async Task GetCustomerById_NotFound_ShouldReturn404()
        {
            var response = await _client.GetAsync("/api/customers/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}
