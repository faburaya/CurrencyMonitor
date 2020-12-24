using System;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.DataModels
{
    public class SubscriptionForExchangeRate
    {
        public int ID { get; set; }

        public string Label { get; set; }
        
        [Display(Name = "E-Mail")]
        public string EMailAddress { get; set; }

        [Display(Name = "Verkaufende Währung")]
        public string CodeCurrencyToSell { get; set; }

        [Display(Name = "Kaufende Währung")]
        public string CodeCurrencyToBuy { get; set; }
    }
}
