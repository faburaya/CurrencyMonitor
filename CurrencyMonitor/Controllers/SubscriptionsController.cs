using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurrencyMonitor.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly Data.CurrencyMonitorContext _context;

        public SubscriptionsController(Data.CurrencyMonitorContext context)
        {
            _context = context;
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index(string emailFilter)
        {
            if (string.IsNullOrWhiteSpace(emailFilter))
            {
                return View(await (
                    from subscription in _context.SubscriptionForExchangeRate
                    select new Models.SubscriptionViewModel(subscription, _context.RecognizedCurrency.ToList())
                ).ToListAsync());
            }
            else
            {
                var searchResults = (
                    from subscription in _context.SubscriptionForExchangeRate
                    where subscription.EMailAddress.Contains(emailFilter)
                    select new Models.SubscriptionViewModel(subscription, _context.RecognizedCurrency.ToList())
                );

                return View(await searchResults.ToListAsync());
            }
        }

        // GET: Subscriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DataModels.SubscriptionForExchangeRate subscription = await _context.SubscriptionForExchangeRate
                .FirstOrDefaultAsync(m => m.ID == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(
                new Models.SubscriptionViewModel(subscription, _context.RecognizedCurrency.ToList())
            );
        }

        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            return View(
                new Models.SubscriptionViewModel(null, _context.RecognizedCurrency.ToList())
            );
        }

        // POST: Subscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Label,EMailAddress,CodeCurrencyToSell,CodeCurrencyToBuy,TargetPriceOfSellingCurrency")] DataModels.SubscriptionForExchangeRate subscriptionForExchangeRate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subscriptionForExchangeRate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subscriptionForExchangeRate);
        }

        // GET: Subscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DataModels.SubscriptionForExchangeRate subscription = await _context.SubscriptionForExchangeRate.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(
                new Models.SubscriptionViewModel(subscription, _context.RecognizedCurrency.ToList())
            );
        }

        // POST: Subscriptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Label,EMailAddress,CodeCurrencyToSell,CodeCurrencyToBuy,TargetPriceOfSellingCurrency")] DataModels.SubscriptionForExchangeRate subscriptionForExchangeRate)
        {
            if (id != subscriptionForExchangeRate.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subscriptionForExchangeRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionForExchangeRateExists(subscriptionForExchangeRate.ID))
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
            return View(subscriptionForExchangeRate);
        }

        // GET: Subscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DataModels.SubscriptionForExchangeRate subscription = await _context.SubscriptionForExchangeRate
                .FirstOrDefaultAsync(m => m.ID == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(
                new Models.SubscriptionViewModel(subscription, _context.RecognizedCurrency.ToList())
            );
        }

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscriptionForExchangeRate = await _context.SubscriptionForExchangeRate.FindAsync(id);
            _context.SubscriptionForExchangeRate.Remove(subscriptionForExchangeRate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionForExchangeRateExists(int id)
        {
            return _context.SubscriptionForExchangeRate.Any(e => e.ID == id);
        }
    }
}
