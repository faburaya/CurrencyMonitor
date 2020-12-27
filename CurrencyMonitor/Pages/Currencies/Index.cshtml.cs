using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Pages.Currencies
{
    public class IndexModel : PageModel
    {
        private readonly CurrencyMonitor.Data.CurrencyMonitorContext _context;

        public IndexModel(CurrencyMonitor.Data.CurrencyMonitorContext context)
        {
            _context = context;
        }

        public IList<RecognizedCurrency> RecognizedCurrency { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                RecognizedCurrency = await _context.RecognizedCurrency.ToListAsync();
            }
            else
            {
                var results = (from currency in _context.RecognizedCurrency
                               where currency.Code.Contains(Filter)
                                || currency.Name.Contains(Filter)
                                || currency.Symbol.Contains(Filter)
                                || currency.Country.Contains(Filter)
                               orderby currency.Code
                               select currency);

                RecognizedCurrency = await results.ToListAsync();
            }
        }

    } // end of class IndexModel
} // end of namespace CurrencyMonitor.Pages.Currencies
