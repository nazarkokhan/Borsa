using System.Text.Json.Serialization;

namespace Borsa.DTO.Alert
{
    public class InstrumentDto
    {
        public InstrumentDto(
            int id, string symbol, double lastPrice, 
            double firstDayPrice, string name, string currency, 
            bool isFavorite, string iconUrl, long dailyVolume)
        {
            Id = id;
            Symbol = symbol;
            LastPrice = lastPrice;
            FirstDayPrice = firstDayPrice;
            Name = name;
            Currency = currency;
            IsFavorite = isFavorite;
            IconUrl = iconUrl;
            DailyVolume = dailyVolume;
        }
        
        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; }

        [JsonPropertyName("lastPrice")]
        public double LastPrice { get; }

        [JsonPropertyName("firstDayPrice")]
        public double FirstDayPrice { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("currency")]
        public string Currency { get; }

        [JsonPropertyName("isFavorite")]
        public bool IsFavorite { get; }

        [JsonPropertyName("iconUrl")]
        public string IconUrl { get; }

        [JsonPropertyName("dailyVolume")]
        public long DailyVolume { get; }
    }
}