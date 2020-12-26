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
    public class DeleteModel : PageModel
    {
        private readonly CurrencyMonitor.Data.RecognizedCurrencyContext _context;

        public DeleteModel(CurrencyMonitor.Data.RecognizedCurrencyContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            RecognizedCurrency = await _context.RecognizedCurrency.FindAsync(id);

            if (RecognizedCurrency != null)
            {
                _context.RecognizedCurrency.Remove(RecognizedCurrency);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
