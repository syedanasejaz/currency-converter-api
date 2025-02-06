namespace CurrencyConverterApi.DTOs
{
    public class HistoricalRate
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
