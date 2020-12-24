using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.Data;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Pages.Subscriptions
{
    public class DeleteModel : PageModel
    {
        private readonly CurrencyMonitor.Data.CurrencyMonitorContext _context;

        public DeleteModel(CurrencyMonitor.Data.CurrencyMonitorContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SubscriptionForExchangeRate SubscriptionForExchangeRate { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SubscriptionForExchangeRate = await _context.SubscriptionForExchangeRate.FirstOrDefaultAsync(m => m.ID == id);

            if (SubscriptionForExchangeRate == null)
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

            SubscriptionForExchangeRate = await _context.SubscriptionForExchangeRate.FindAsync(id);

            if (SubscriptionForExchangeRate != null)
            {
                _context.SubscriptionForExchangeRate.Remove(SubscriptionForExchangeRate);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
