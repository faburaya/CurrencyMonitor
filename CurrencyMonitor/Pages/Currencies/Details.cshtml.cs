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
    public class DetailsModel : PageModel
    {
        private readonly CurrencyMonitor.Data.RecognizedCurrencyContext _context;

        public DetailsModel(CurrencyMonitor.Data.RecognizedCurrencyContext context)
        {
            _context = context;
        }

        public RecognizedCurrency RecognizedCurrency { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            RecognizedCurrency = await _context.RecognizedCurrency.FirstOrDefaultAsync(m => m.ID == id);

            if (RecognizedCurrency == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
