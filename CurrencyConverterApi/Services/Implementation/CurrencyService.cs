using CurrencyConverterApi.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System.Globalization;

namespace CurrencyConverterApi.Services.Implementation
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private static readonly HashSet<string> ExcludedCurrencies = new() { "TRY", "PLN", "THB", "MXN" };
        private readonly IMemoryCache _cache; // Caching for repeated requests

        public CurrencyService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential Backoff
        }

        // Endpoint 1: Retrieve the latest exchange rates for a specific base currency
        public async Task<Dictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency)
        {
            if (_cache.TryGetValue(baseCurrency, out Dictionary<string, decimal> cachedRates))
            {
                return cachedRates; // Return cached data if available
            }

            var url = $"https://api.frankfurter.app/latest?from={baseCurrency}";
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve latest rates. Status: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var rates = JsonConvert.DeserializeObject<FrankfurterRates>(json);
            var ratesDict = rates?.Rates ?? new Dictionary<string, decimal>();

            // Cache the result for 30 minutes to optimize performance
            _cache.Set(baseCurrency, ratesDict, TimeSpan.FromMinutes(30));

            return ratesDict;
        }

        // Endpoint 2: Convert an amount between different currencies (excluding specific ones)
        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (ExcludedCurrencies.Contains(toCurrency))
            {
                throw new InvalidOperationException($"Currency conversion for {toCurrency} is not allowed.");
            }

            var rates = await GetLatestRatesAsync(fromCurrency);
            if (!rates.TryGetValue(toCurrency, out var rate))
            {
                throw new KeyNotFoundException($"Currency {toCurrency} not found in rates.");
            }

            return amount * rate;
        }

        // Endpoint 3: Get historical rates for a specific period with pagination
        public async Task<List<HistoricalRate>> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
        {
            var url = $"https://api.frankfurter.app/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?from={baseCurrency}";
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve historical rates. Status: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var rates = JsonConvert.DeserializeObject<HistoricalRatesResponse>(json);

            var historicalRates = rates?.Rates
                .Select(r => new HistoricalRate
                {
                    Date = DateTime.ParseExact(r.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture), // Parse the date string to DateTime
                    Rates = r.Value
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return historicalRates ?? new List<HistoricalRate>();
        }
    }
}
