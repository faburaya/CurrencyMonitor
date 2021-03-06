﻿using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.Models
{
    /// <summary>
    /// View-Model des Abonnements für Beobachtung des Wechselkurses.
    /// </summary>
    public class SubscriptionViewModel : DataModels.SubscriptionForExchangeRate
    {
        private SubscriptionViewModel(IEnumerable<DataModels.RecognizedCurrency> recognizedCurrencies)
        {
            this.AvailableCurrencies = (from currency
                                        in recognizedCurrencies
                                        orderby currency.Code
                                        select new SelectListItem
                                        {
                                            Value = currency.Code,
                                            Text = $"{currency.Code} - {currency.Name}"
                                        });
        }

        public SubscriptionViewModel(
            DataModels.SubscriptionForExchangeRate subscription,
            IEnumerable<DataModels.RecognizedCurrency> recognizedCurrencies)
            : this(recognizedCurrencies)
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
        }

        public SubscriptionViewModel(
            string codeCurrencyToSell,
            string codeCurrencyToBuy,
            IEnumerable<DataModels.RecognizedCurrency> recognizedCurrencies)
            : this(recognizedCurrencies)
        {
            this.CodeCurrencyToSell = codeCurrencyToSell;
            this.CodeCurrencyToBuy = codeCurrencyToBuy;
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

    }// end of class SubscriptionViewModel

}// end of namespace CurrencyMonitor.Models
