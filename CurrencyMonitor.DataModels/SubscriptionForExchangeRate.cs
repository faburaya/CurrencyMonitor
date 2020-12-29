using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Stellt ein Abonnement von einem Benutzer (Besitzer des E-Mail),
    /// das den Wechselkurs beobachtet.
    /// </summary>
    public class SubscriptionForExchangeRate
    {
        public int ID { get; set; }

        [Display(Name = "Bezeichnung")]
        public string Label { get; set; }

        [Display(Name = "E-Mail")]
        public string EMailAddress { get; set; }

        [Display(Name = "Verkaufende Währung")]
        public string CodeCurrencyToSell { get; set; }

        [Display(Name = "Kaufende Währung")]
        public string CodeCurrencyToBuy { get; set; }

        [Display(Name = "Erzielter Preis der verkaufenden Währung")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TargetPriceOfSellingCurrency { get; set; }
    }
}
