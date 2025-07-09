using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankWebApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly ICustomerServices _customerServices;
        public CustomersController(ILogger<CustomersController> logger, ICustomerServices customerServices)
        {
            _logger = logger;
            _customerServices = customerServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] JsonElement customerJson)
        {
            if (!customerJson.TryGetProperty("name", out var nameProp) || string.IsNullOrWhiteSpace(nameProp.GetString()))
                return BadRequest("Name is required.");
            if (!customerJson.TryGetProperty("dateOfBirth", out var dobProp) || dobProp.ValueKind != JsonValueKind.String)
                return BadRequest("DateOfBirth is required.");
            if (!DateTime.TryParse(dobProp.GetString(), out var dateOfBirth))
                return BadRequest("DateOfBirth must be a valid date.");
            if (!customerJson.TryGetProperty("gender", out var genderProp) || string.IsNullOrWhiteSpace(genderProp.GetString()))
                return BadRequest("Gender is required.");
            if (!customerJson.TryGetProperty("income", out var incomeProp) || !incomeProp.TryGetDecimal(out var income) || income < 0)
                return BadRequest("Income must be a non-negative decimal.");

            try
            {
                var dto = new CreateCustomerDto
                {
                    Name = nameProp.GetString(),
                    DateOfBirth = dateOfBirth,
                    Gender = genderProp.GetString(),
                    Income = income
                };
                var createdCustomer = await _customerServices.CreateCustomerAsync(dto);
                return CreatedAtAction(nameof(CreateCustomer), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating customer");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerServices.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }
    }
}

