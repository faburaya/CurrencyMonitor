using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            IEnumerable<DataModels.ExchangeRate> exchangeRates;
            if (string.IsNullOrWhiteSpace(currencyFilter))
            {
                exchangeRates = await
                    _dbServiceExchangeRates.QueryAsync(source => source.Select(item => item));
            }
            else
            {
                currencyFilter = currencyFilter.ToUpper();
                exchangeRates = await _dbServiceExchangeRates.QueryAsync(source =>
                    source.Where(item =>
                        item.PrimaryCurrencyCode.Contains(currencyFilter)
                        || item.SecondaryCurrencyCode.Contains(currencyFilter)
                    ).Select(item => item)
                );
            }

            // verdoppelt alle Wechselkurse, um auch die umgekehrte Richtung der Wechselkurse einzuschließen:
            var entries = new List<Models.ExchangeRateViewModel>(capacity: 2 * exchangeRates.Count());
            foreach (DataModels.ExchangeRate exchangeRate in exchangeRates)
            {
                entries.Add(new Models.ExchangeRateViewModel(exchangeRate, false));
                entries.Add(new Models.ExchangeRateViewModel(exchangeRate, true));
            }
            entries.Sort((a, b) => a.ExchangeRate.CompareTo(b.ExchangeRate));

            return View(entries);
        }

    }// end of class ExchangeRatesController

}// end of namespace CurrencyMonitor.Controllers
