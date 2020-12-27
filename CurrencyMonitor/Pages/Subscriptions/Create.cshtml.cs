using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CurrencyMonitor.Pages.Subscriptions
{
    public class CreateModel : PageModel
    {
        private readonly Data.CurrencyMonitorContext _context;

        public CreateModel(Data.CurrencyMonitorContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            AvailableCurrencies = (
                from currency in _context.RecognizedCurrency
                orderby currency.Code
                select new SelectListItem {
                    Value = currency.Code,
                    Text = $"{currency.Code} : {currency.Name}"
                });

            return Page();
        }

        [BindProperty]
        public DataModels.SubscriptionForExchangeRate SubscriptionForExchangeRate { get; set; }

        public IEnumerable<SelectListItem> AvailableCurrencies { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.SubscriptionForExchangeRate.Add(SubscriptionForExchangeRate);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
