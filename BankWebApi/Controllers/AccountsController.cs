using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankWebApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly IAccountServices _accountServices;
        private readonly ICustomerServices _customerServices;

        public AccountsController(ILogger<AccountsController> logger, IAccountServices accountServices, ICustomerServices customerServices)
        {
            _logger = logger;
            _accountServices = accountServices;
            _customerServices = customerServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] JsonElement accountDtoJson)
        {
            // Permitir clientId o customerId
            int clientId = 0;
            if (accountDtoJson.TryGetProperty("clientId", out var clientIdProp))
                clientId = clientIdProp.GetInt32();
            else if (accountDtoJson.TryGetProperty("customerId", out var customerIdProp))
                clientId = customerIdProp.GetInt32();
            else
                return BadRequest("ClientId or CustomerId is required.");

            if (!accountDtoJson.TryGetProperty("accountNumber", out var accNumProp) || string.IsNullOrWhiteSpace(accNumProp.GetString()))
                return BadRequest("AccountNumber is required.");
            string accountNumber = accNumProp.GetString();

            decimal balance = 0;
            if (accountDtoJson.TryGetProperty("balance", out var balProp))
            {
                if (!balProp.TryGetDecimal(out balance) || balance < 0)
                    return BadRequest("Balance must be a non-negative decimal.");
            }
            else
            {
                return BadRequest("Balance is required.");
            }

            DateTime createdAt = accountDtoJson.TryGetProperty("createdAt", out var createdAtProp) && createdAtProp.ValueKind == JsonValueKind.String ? DateTime.Parse(createdAtProp.GetString()) : DateTime.UtcNow;

            if (clientId <= 0)
            {
                return BadRequest("ClientId must be a positive integer.");
            }
            var clientExists = await _customerServices.CustomerExistsAsync(clientId);
            if (!clientExists)
            {
                return NotFound("Client not found.");
            }
            // Validar unicidad del número de cuenta
            bool accountExists = false;
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                try
                {
                    var _ = await _accountServices.GetBalanceByAccountNumberAsync(accountNumber);
                    accountExists = true;
                }
                catch (KeyNotFoundException)
                {
                    accountExists = false;
                }
            }
            if (accountExists)
            {
                return Conflict("Account number already exists.");
            }
            try
            {
                var dto = new CreateAccountDto
                {
                    ClientId = clientId,
                    AccountNumber = accountNumber,
                    Balance = balance,
                    CreatedAt = createdAt
                };
                var createdAccount = await _accountServices.CreateAccountAsync(dto);
                return CreatedAtAction(nameof(CreateAccount), new { id = createdAccount.Id }, new {
                    id = createdAccount.Id,
                    accountNumber = createdAccount.AccountNumber,
                    clientId = createdAccount.ClientId,
                    balance = createdAccount.Balance,
                    createdAt = createdAccount.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating account");
            }
        }

        [HttpGet("balance/{accountNumber?}")]
        public async Task<IActionResult> GetBalance(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest("Account number is required.");
            }
            try
            {
                var balance = await _accountServices.GetBalanceByAccountNumberAsync(accountNumber);
                return Ok(new { AccountNumber = accountNumber, Balance = balance });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Account not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for account {AccountNumber}", accountNumber);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving balance");
            }
        }
        [HttpPost("apply-interest/{accountNumber}")]
        public async Task<IActionResult> ApplyInterest(string accountNumber, [FromQuery] decimal annualRate)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Account number is required.");
            }
            if (annualRate <= 0)
            {
                return BadRequest("Annual interest rate must be greater than zero.");
            }
            try
            {
                var updatedAccount = await _accountServices.ApplyInterestAsync(accountNumber, annualRate);
                return Ok(updatedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying interest to account {AccountNumber}", accountNumber);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error applying interest");
            }
        }


    }
}
