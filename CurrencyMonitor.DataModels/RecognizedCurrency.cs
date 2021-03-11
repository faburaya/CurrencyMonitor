using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using Reusable.DataModels;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Stellt eine anerkannte Währung, etwa Real oder Euro.
    /// </summary>
    [CosmosContainer(Name = "Währung")]
    public class RecognizedCurrency : CosmosDbItem<RecognizedCurrency>, IEquatable<RecognizedCurrency>
    {
        /// <summary>
        /// Code nach ISO-4217.
        /// </summary>
        [Required]
        [CosmosPartitionKey]
        [RegularExpression(@"^[A-Z]{3}$")]
        [Display(Name = "ISO-Code")]
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "Währung")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [Display(Name = "Symbol")]
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [Display(Name = "Gültigkeit (Länder)")]
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        public bool Equals(RecognizedCurrency other)
        {
            return this.Code == other.Code
                && this.Name == other.Name
                && this.Symbol == other.Symbol
                && this.Country == other.Country;
        }

    }// end of class RecognizedCurrency

}// end of namespace CurrencyMonitor.DataModels
