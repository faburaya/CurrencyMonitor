using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.Data;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Pages.Subscriptions
{
    public class EditModel : PageModel
    {
        private readonly CurrencyMonitor.Data.CurrencyMonitorContext _context;

        public EditModel(CurrencyMonitor.Data.CurrencyMonitorContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(SubscriptionForExchangeRate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubscriptionForExchangeRateExists(SubscriptionForExchangeRate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool SubscriptionForExchangeRateExists(int id)
        {
            return _context.SubscriptionForExchangeRate.Any(e => e.ID == id);
        }
    }
}
