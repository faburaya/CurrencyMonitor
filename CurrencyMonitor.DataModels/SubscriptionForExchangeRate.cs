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

        [Required]
        [Display(Name = "Bezeichnung")]
        public string Label { get; set; }

        [Required]
        [RegularExpression(@"^[^@]+@\w+(\.\w+)+$")]
        [Display(Name = "E-Mail")]
        public string EMailAddress { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}$")]
        [Display(Name = "Verkaufende Währung")]
        public string CodeCurrencyToSell { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}$")]
        [Display(Name = "Kaufende Währung")]
        public string CodeCurrencyToBuy { get; set; }

        [Required]
        [Display(Name = "Erzielter Preis der verkaufenden Währung")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TargetPriceOfSellingCurrency { get; set; }
    }
}
