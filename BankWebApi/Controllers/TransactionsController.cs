using BankWebApi.Models.DTOs;
using BankWebApi.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BankWebApi.Models;

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
        public async Task<IActionResult> RegisterTransaction([FromBody] JsonElement transactionDtoJson)
        {
            if (!transactionDtoJson.TryGetProperty("accountNumber", out var accNumProp) || string.IsNullOrWhiteSpace(accNumProp.GetString()))
                return BadRequest("AccountNumber is required.");
            if (!transactionDtoJson.TryGetProperty("transactionType", out var typeProp) || string.IsNullOrWhiteSpace(typeProp.GetString()))
                return BadRequest("TransactionType is required.");
            if (!transactionDtoJson.TryGetProperty("amount", out var amountProp) || !amountProp.TryGetDecimal(out var amount) || amount <= 0)
                return BadRequest("Amount must be a positive decimal.");

            var accountNumber = accNumProp.GetString();
            var transactionTypeStr = typeProp.GetString();
            if (!Enum.TryParse<TransactionType>(transactionTypeStr, true, out var transactionType))
                return BadRequest($"TransactionType '{transactionTypeStr}' is invalid.");

            try
            {
                var result = await _transactionServices.RegisterTransactionAsync(
                    accountNumber,
                    transactionType,
                    amount);
                return CreatedAtAction(nameof(RegisterTransaction), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("fondos", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("insufficient", StringComparison.OrdinalIgnoreCase))
                {
                    return UnprocessableEntity("Insufficient funds for this transaction.");
                }
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound("Account not found.");
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

