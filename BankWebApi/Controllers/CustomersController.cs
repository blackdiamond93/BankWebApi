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
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (await _customerServices.CustomerExistsByDataAsync(dto))
                    return Conflict("A customer with the same data already exists.");
                var createdCustomer = await _customerServices.CreateCustomerAsync(dto);
                var response = new { id = createdCustomer.Id, dto.Name, dto.DateOfBirth, dto.Gender, dto.Income };
                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.Id }, response);
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
            var response = new { id = customer.Id, customer.Name, customer.DateOfBirth, customer.Gender, customer.Income };
            return Ok(response);
        }
    }
}

