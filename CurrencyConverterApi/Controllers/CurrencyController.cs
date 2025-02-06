using CurrencyConverterApi.Services.Implementation;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApi.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly CurrencyService _currencyService;

        public CurrencyController(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        // Endpoint 1: Retrieve the latest exchange rates for a specific base currency
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRates([FromQuery] string baseCurrency)
        {
            try
            {
                var rates = await _currencyService.GetLatestRatesAsync(baseCurrency);
                return Ok(new { BaseCurrency = baseCurrency, Rates = rates });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Endpoint 2: Convert an amount between different currencies (excluding specific ones)
        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency(
            [FromQuery] decimal amount,
            [FromQuery] string fromCurrency,
            [FromQuery] string toCurrency)
        {
            try
            {
                var convertedAmount = await _currencyService.ConvertCurrencyAsync(amount, fromCurrency, toCurrency);
                return Ok(new { From = fromCurrency, To = toCurrency, Amount = amount, ConvertedAmount = convertedAmount });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Details = ex.Message });
            }
        }

        // Endpoint 3: Get historical rates with pagination
        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalRates(
            [FromQuery] string baseCurrency,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var rates = await _currencyService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
                return Ok(new { BaseCurrency = baseCurrency, StartDate = startDate, EndDate = endDate, Rates = rates });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
