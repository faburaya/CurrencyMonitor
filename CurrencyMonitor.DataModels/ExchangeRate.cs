using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using Reusable.DataModels;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Hält einen Wert des Wechselkurses.
    /// </summary>
    [CosmosContainer(Name = "Wechselkurs")]
    public class ExchangeRate : CosmosDbItem
    {
        public ExchangeRate(ExchangePair exchange, double rate)
        {
            this.PrimaryCurrencyCode = exchange.PrimaryCurrencyCode;
            this.SecondaryCurrencyCode = exchange.SecondaryCurrencyCode;
            this.PriceOfPrimaryCurrency = rate;
        }

        public override string PartitionKeyValue => CosmosDbPartitionedItem<ExchangeRate>.GetPartitionKeyValue(this);

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
        /// Preis der primären Währung.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "rate")]
        public double PriceOfPrimaryCurrency { get; set; }

    }// end of class ExchangeRate

}// end of namespace CurrencyMonitor.DataModels
