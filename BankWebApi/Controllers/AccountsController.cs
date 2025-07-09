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
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (dto.ClientId <= 0)
                return BadRequest("ClientId must be a positive integer.");
            if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                return BadRequest("AccountNumber is required.");
            if (dto.Balance < 0)
                return BadRequest("Balance must be a non-negative decimal.");
            var clientExists = await _customerServices.CustomerExistsAsync(dto.ClientId);
            if (!clientExists)
                return NotFound("Client not found.");
            // Validar unicidad del número de cuenta
            bool accountExists = false;
            try
            {
                var _ = await _accountServices.GetBalanceByAccountNumberAsync(dto.AccountNumber);
                accountExists = true;
            }
            catch (KeyNotFoundException)
            {
                accountExists = false;
            }
            if (accountExists)
                return Conflict("Account number already exists.");
            try
            {
                var createdAccount = await _accountServices.CreateAccountAsync(dto);
                var response = new {
                    id = createdAccount.Id,
                    accountNumber = createdAccount.AccountNumber,
                    clientId = createdAccount.ClientId,
                    balance = createdAccount.Balance,
                    createdAt = createdAccount.CreatedAt
                };
                return CreatedAtAction(nameof(CreateAccount), new { id = createdAccount.Id }, response);
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
