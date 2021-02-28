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
    public class SubscriptionForExchangeRate : CosmosDbItem
    {
        public override string PartitionKeyValue =>
            CosmosDbPartitionedItem<SubscriptionForExchangeRate>.GetPartitionKeyValue(this);

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
    }
}
