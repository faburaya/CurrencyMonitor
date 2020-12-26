using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CurrencyMonitor.Data;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Pages.Currencies
{
    public class CreateModel : PageModel
    {
        private readonly CurrencyMonitor.Data.RecognizedCurrencyContext _context;

        public CreateModel(CurrencyMonitor.Data.RecognizedCurrencyContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public RecognizedCurrency RecognizedCurrency { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.RecognizedCurrency.Add(RecognizedCurrency);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
