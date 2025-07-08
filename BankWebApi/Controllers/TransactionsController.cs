using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankWebApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly ITransactionServices _transactionServices;

        public TransactionsController(ILogger<TransactionsController> logger, ITransactionServices transactionServices)
        {
            _logger = logger;
            _transactionServices = transactionServices;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterTransaction([FromBody] RegisterTransactionDto transactionDto)
        {
            if (transactionDto == null)
            {
                return BadRequest("Transaction data is null.");
            }
            try
            {
                var result = await _transactionServices.RegisterTransactionAsync(
                    transactionDto.AccountNumber,
                    transactionDto.TransactionType,
                    transactionDto.Amount);
                return CreatedAtAction(nameof(RegisterTransaction), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Insufficient funds for account {AccountNumber}", transactionDto.AccountNumber);
                return BadRequest("Insufficient funds for this transaction.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering transaction");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error registering transaction");
            }
        }


        [HttpGet("history/{accountNumber}")]
        public async Task<IActionResult> GetTransactionHistory(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Account number is required.");
            }
            try
            {
                var transactions = await _transactionServices.GetHistoryByAccountAsync(accountNumber);
                return Ok(transactions);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Account not found: {AccountNumber}", accountNumber);
                return NotFound("Account not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction history for account {AccountNumber}", accountNumber);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving transaction history");
            }
        }

        [HttpGet("Summary/{accountNumber}")]
        public async Task<IActionResult> GetTransactionSummary(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Account number is required.");
            }
            try
            {
                var summary = await _transactionServices.GetAccountSummaryByAccountNumberAsync(accountNumber);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Account not found: {AccountNumber}", accountNumber);
                return NotFound("Account not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction summary for account {AccountNumber}", accountNumber);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving transaction summary");
            }
        }
    }
}
