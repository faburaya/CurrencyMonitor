using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.Data;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Controllers
{
    public class CurrenciesController : Controller
    {
        private readonly CurrencyMonitorContext _context;

        public CurrenciesController(CurrencyMonitorContext context)
        {
            _context = context;
        }

        // GET: Currencies
        public async Task<IActionResult> Index(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(await _context.RecognizedCurrency.ToListAsync());
            }
            else
            {
                var results = (from currency in _context.RecognizedCurrency
                               where currency.Code.Contains(searchString)
                                || currency.Name.Contains(searchString)
                                || currency.Symbol.Contains(searchString)
                                || currency.Country.Contains(searchString)
                               orderby currency.Code
                               select currency);

                return View(await results.ToListAsync());
            }
        }

        // GET: Currencies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recognizedCurrency = await _context.RecognizedCurrency
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recognizedCurrency == null)
            {
                return NotFound();
            }

            return View(recognizedCurrency);
        }

        // GET: Currencies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Currencies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Code,Name,Symbol,Country")] RecognizedCurrency recognizedCurrency)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recognizedCurrency);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(recognizedCurrency);
        }

        // GET: Currencies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recognizedCurrency = await _context.RecognizedCurrency.FindAsync(id);
            if (recognizedCurrency == null)
            {
                return NotFound();
            }
            return View(recognizedCurrency);
        }

        // POST: Currencies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Code,Name,Symbol,Country")] RecognizedCurrency recognizedCurrency)
        {
            if (id != recognizedCurrency.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recognizedCurrency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecognizedCurrencyExists(recognizedCurrency.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recognizedCurrency);
        }

        // GET: Currencies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recognizedCurrency = await _context.RecognizedCurrency
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recognizedCurrency == null)
            {
                return NotFound();
            }

            return View(recognizedCurrency);
        }

        // POST: Currencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recognizedCurrency = await _context.RecognizedCurrency.FindAsync(id);
            _context.RecognizedCurrency.Remove(recognizedCurrency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecognizedCurrencyExists(int id)
        {
            return _context.RecognizedCurrency.Any(e => e.ID == id);
        }
    }
}
