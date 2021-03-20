using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using Reusable.DataModels;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Hält einen Wert des Wechselkurses.
    /// </summary>
    [CosmosContainer(Name = "Wechselkurs")]
    public class ExchangeRate : CosmosDbItem<ExchangeRate>, IEquatable<ExchangeRate>
    {
        /// <remarks>
        /// Hier erstellt man das Objekt durch eine statische Methode,
        /// denn ein Konstruktor verursacht Probleme bei der Deserialisierung,
        /// wenn man ein Element aus der Datenbank holt.
        /// </remarks>
        public static ExchangeRate CreateFrom(ExchangePair exchange, double rate)
        {
            return new ExchangeRate {
                PrimaryCurrencyCode = exchange.PrimaryCurrencyCode,
                SecondaryCurrencyCode = exchange.SecondaryCurrencyCode,
                PriceOfPrimaryCurrency = rate,
                Timestamp = DateTime.Now.ToUniversalTime()
            };
        }

        /// <summary>
        /// Code der primären Währung nach ISO-4217.
        /// </summary>
        [Required]
        [CosmosPartitionKey]
        [RegularExpression(@"^[A-Z]{3}$")]
        [JsonProperty(PropertyName = "currency1")]
        public string PrimaryCurrencyCode { get; set; }

        /// <summary>
        /// Code der sekundären Währung nach ISO-4217.
        /// </summary>
        [Required]
        [RegularExpression(@"^[A-Z]{3}$")]
        [JsonProperty(PropertyName = "currency2")]
        public string SecondaryCurrencyCode { get; set; }

        /// <summary>
        /// Preis der primären Währung
        /// (wenn sie mit der sekundären Währung gekauft wird.)
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "rate")]
        public double PriceOfPrimaryCurrency { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }

        public bool Equals(ExchangeRate other)
        {
            return this.PrimaryCurrencyCode == other.PrimaryCurrencyCode
                && this.SecondaryCurrencyCode == other.SecondaryCurrencyCode
                && this.PriceOfPrimaryCurrency == other.PriceOfPrimaryCurrency
                && this.Timestamp == other.Timestamp;
        }
    }// end of class ExchangeRate

}// end of namespace CurrencyMonitor.DataModels
