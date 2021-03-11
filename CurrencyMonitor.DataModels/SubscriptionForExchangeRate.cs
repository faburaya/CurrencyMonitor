using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

using Reusable.DataModels;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Stellt ein Abonnement von einem Benutzer (Besitzer des E-Mail),
    /// das den Wechselkurs beobachtet.
    /// </summary>
    [CosmosContainer(Name = "Abonnement")]
    public class SubscriptionForExchangeRate
        : CosmosDbItem<SubscriptionForExchangeRate>
        , IEquatable<SubscriptionForExchangeRate>
    {
        [Required]
        [Display(Name = "Bezeichnung")]
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [Required]
        [CosmosPartitionKey]
        [RegularExpression(@"^[^@]+@\w+(\.\w+)+$")]
        [Display(Name = "E-Mail")]
        [JsonProperty(PropertyName = "email")]
        public string EMailAddress { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}$")]
        [Display(Name = "Verkaufende Währung")]
        [JsonProperty(PropertyName = "currencyToSell")]
        public string CodeCurrencyToSell { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}$")]
        [Display(Name = "Kaufende Währung")]
        [JsonProperty(PropertyName = "currencyToBuy")]
        public string CodeCurrencyToBuy { get; set; }

        [Required]
        [Display(Name = "Erzielter Preis der verkaufenden Währung")]
        [Column(TypeName = "decimal(10, 2)")]
        [JsonProperty(PropertyName = "targetPrice")]
        public decimal TargetPriceOfSellingCurrency { get; set; }

        public bool Equals(SubscriptionForExchangeRate other)
        {
            return this.Label == other.Label
                && this.EMailAddress == other.EMailAddress
                && this.CodeCurrencyToSell == other.CodeCurrencyToSell
                && this.CodeCurrencyToBuy == other.CodeCurrencyToBuy
                && this.TargetPriceOfSellingCurrency == other.TargetPriceOfSellingCurrency;
        }

    }// end of class SubscriptionForExchangeRate

}// end of namespace CurrencyMonitor.DataModels
