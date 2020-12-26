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

namespace CurrencyMonitor.Pages.Currencies
{
    public class EditModel : PageModel
    {
        private readonly CurrencyMonitor.Data.RecognizedCurrencyContext _context;

        public EditModel(CurrencyMonitor.Data.RecognizedCurrencyContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(RecognizedCurrency).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecognizedCurrencyExists(RecognizedCurrency.ID))
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

        private bool RecognizedCurrencyExists(int id)
        {
            return _context.RecognizedCurrency.Any(e => e.ID == id);
        }
    }
}
