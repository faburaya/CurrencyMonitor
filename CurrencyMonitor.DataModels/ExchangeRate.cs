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
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Hilft der Erstellung von einem Objekt, sodass man den Aufruf sich besser verstehen lässt.
        /// Das verbessert die Lesbarkeit der Tests.
        /// </summary>
        /// <param name="amount1">Der Betrag in Währung 1.</param>
        /// <param name="currencyCode1">Der Code für Währung 1.</param>
        /// <param name="amount2">Der Betrag in Währung 2, der dem Betrag in Währung 1 entspricht.</param>
        /// <param name="currencyCode2">Der Code für Währung 2.</param>
        /// <returns>Der aus den angegebenen Parametern erstellte Wechselkurs.</returns>
        public static ExchangeRate CreateFrom(double amount1, string currencyCode1,
                                              double amount2, string currencyCode2)
        {
            var pair = new ExchangePair(currencyCode1, currencyCode2);
            
            double rate;
            if (pair.PrimaryCurrencyCode == currencyCode1)
            {
                rate = amount2 / amount1;
            }
            else
            {
                rate = amount1 / amount2;
            }

            return CreateFrom(pair, rate);
        }

        public ExchangeRate Revert()
        {
            return new ExchangeRate
            {
                PrimaryCurrencyCode = this.SecondaryCurrencyCode,
                SecondaryCurrencyCode = this.PrimaryCurrencyCode,
                PriceOfPrimaryCurrency = 1.0 / this.PriceOfPrimaryCurrency,
                Timestamp = this.Timestamp
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
