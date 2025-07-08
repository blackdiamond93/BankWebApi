using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankWebApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly IAccountServices _accountServices;
        public AccountsController(ILogger<AccountsController> logger, IAccountServices accountServices)
        {
            _logger = logger;
            _accountServices = accountServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto accountDto)
        {
            if (accountDto == null)
            {
                return BadRequest("Account data is null.");
            }
            try
            {
                var createdAccount = await _accountServices.CreateAccountAsync(accountDto);
                return CreatedAtAction(nameof(CreateAccount), new { id = createdAccount.Id }, createdAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating account");
            }
        }

        [HttpGet("balance/{accountNumber}")]
        public async Task<IActionResult> GetBalance(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Account number is required.");
            }
            try
            {
                var balance = await _accountServices.GetBalanceByAccountNumberAsync(accountNumber);
                return Ok(new { AccountNumber = accountNumber, Balance = balance });
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
