namespace CurrencyConverterApi.DTOs
{
    public class HistoricalRatesResponse
    {
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }
}
