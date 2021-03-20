using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Reusable.DataAccess;

namespace CurrencyMonitor.Controllers
{
    public class ExchangeRatesController : Controller
    {
        private readonly ICosmosDbService<DataModels.ExchangeRate> _dbServiceExchangeRates;

        public ExchangeRatesController(ICosmosDbService<DataModels.ExchangeRate> dbServiceExchangeRates)
        {
            _dbServiceExchangeRates = dbServiceExchangeRates;
        }

        // GET: ExchangeRatesController
        public async Task<IActionResult> Index(string currencyFilter)
        {
            var allExchangeRates =
                await _dbServiceExchangeRates.QueryAsync(source => source.Select(rate => rate));

            // verdoppelt alle Wechselkurse, um auch die umgekehrte Richtung der Wechselkurse einzuschließen:
            var entries = new List<Models.ExchangeRateViewModel>(capacity: 2 * allExchangeRates.Count());
            foreach (DataModels.ExchangeRate exchangeRate in allExchangeRates)
            {
                entries.Add(new Models.ExchangeRateViewModel(exchangeRate, false));
                entries.Add(new Models.ExchangeRateViewModel(exchangeRate, true));
            }

            if (string.IsNullOrWhiteSpace(currencyFilter))
            {
                return View(entries);
            }

            currencyFilter = currencyFilter.ToUpper();
            return View(from entry in entries
                        where entry.ExchangeRate.Contains(currencyFilter)
                        select entry);
        }

    }// end of class ExchangeRatesController

}// end of namespace CurrencyMonitor.Controllers
