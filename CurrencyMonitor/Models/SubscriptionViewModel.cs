using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.Models
{
    /// <summary>
    /// View-Model des Abonnements für Beobachtung des Wechselkurses.
    /// Die readonly Felder dienen spezifischen Voraussetzungen der Ansicht.
    /// </summary>
    public class SubscriptionViewModel : DataModels.SubscriptionForExchangeRate
    {
        public SubscriptionViewModel(DataModels.SubscriptionForExchangeRate subscription, IEnumerable<DataModels.RecognizedCurrency> recognizedCurrencies)
        {
            if (subscription != null)
            {
                this.Id = subscription.Id;
                this.Label = subscription.Label;
                this.EMailAddress = subscription.EMailAddress;
                this.CodeCurrencyToSell = subscription.CodeCurrencyToSell;
                this.CodeCurrencyToBuy = subscription.CodeCurrencyToBuy;
                this.TargetPriceOfSellingCurrency = subscription.TargetPriceOfSellingCurrency; 
            }

            this.AvailableCurrencies = (from currency
                                        in recognizedCurrencies
                                        orderby currency.Code
                                        select new SelectListItem
                                        {
                                            Value = currency.Code,
                                            Text = $"{currency.Code} - {currency.Name}"
                                        });
        }

        public IEnumerable<SelectListItem> AvailableCurrencies { get; }

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
