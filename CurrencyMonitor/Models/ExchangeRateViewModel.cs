using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.Models
{
    /// <summary>
    /// View-Model für die Liste von Wechselkursen.
    /// </summary>
    public class ExchangeRateViewModel
    {
        public ExchangeRateViewModel(DataModels.ExchangeRate exchangeRate, bool mustRevert)
        {
            double price;

            if (!mustRevert)
            {
                this.CodeOfSellingCurrency = exchangeRate.SecondaryCurrencyCode;
                this.CodeOfBuyingCurrency = exchangeRate.PrimaryCurrencyCode;
                price = exchangeRate.PriceOfPrimaryCurrency;
            }
            else
            {
                this.CodeOfSellingCurrency = exchangeRate.PrimaryCurrencyCode;
                this.CodeOfBuyingCurrency = exchangeRate.SecondaryCurrencyCode;
                price = 1.0 / exchangeRate.PriceOfPrimaryCurrency;
            }

            this.ExchangeRate = $"1 {CodeOfBuyingCurrency} = {price:F2} {CodeOfSellingCurrency}";
            this.Timestamp = exchangeRate.Timestamp;
        }

        public string CodeOfSellingCurrency { get; }

        public string CodeOfBuyingCurrency { get; }

        [Display(Name = "Wechselkurs")]
        public string ExchangeRate { get; }

        [Display(Name = "Letzte Erfassung")]
        public DateTime Timestamp { get; }
    }
}
