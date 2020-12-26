using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.Data;
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

        public IList<RecognizedCurrency> RecognizedCurrency { get;set; }

        public async Task OnGetAsync()
        {
            RecognizedCurrency = await _context.RecognizedCurrency.ToListAsync();
        }
    }
}
