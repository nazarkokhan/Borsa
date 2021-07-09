using System;
using System.Text.Json.Serialization;

namespace Borsa
{
    public class Tick
    {
        public Tick(string symbol, double price, DateTime dateTime, long volume, string country, long time)
        {
            Symbol = symbol;
            Price = price;
            DateTime = dateTime;
            Volume = volume;
            Country = country;
            Time = time;
        }

        [JsonPropertyName("symbol")]
        public string Symbol { get; }

        [JsonPropertyName("price")]
        public double Price { get;  }

        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get;  }

        [JsonPropertyName("volume")]
        public long Volume { get;  }

        [JsonPropertyName("country")]
        public string Country { get;  }

        [JsonPropertyName("time")]
        public long Time { get;  }
    }
}