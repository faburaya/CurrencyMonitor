using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.Models
{
    /// <summary>
    /// View-Model des Abonnements für Beobachtung des Wechselkurses.
    /// Die readonly Felder dienen spezifische Voraussetzungen der Ansicht.
    /// </summary>
    public class SubscriptionViewModel : DataModels.SubscriptionForExchangeRate
    {
        public SubscriptionViewModel(DataModels.SubscriptionForExchangeRate data)
        {
            this.ID = data.ID;
            this.Label = data.Label;
            this.EMailAddress = data.EMailAddress;
            this.CodeCurrencyToSell = data.CodeCurrencyToSell;
            this.CodeCurrencyToBuy = data.CodeCurrencyToBuy;
            this.TargetPriceOfSellingCurrency = data.TargetPriceOfSellingCurrency;
        }

        [Display(Name = "Wechsel")]
        public string Exchange
        {
            get { return $"{CodeCurrencyToSell} -> {CodeCurrencyToBuy}"; }
        }

        [Display(Name = "Erzielter Preis")]
        public string TargetLabel
        {
            get { return $"{TargetPriceOfSellingCurrency} {CodeCurrencyToBuy}"; }
        }
    }
}
