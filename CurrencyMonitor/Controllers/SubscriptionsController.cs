using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Reusable.DataAccess;

namespace CurrencyMonitor.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly ICosmosDbService<DataModels.SubscriptionForExchangeRate> _dbServiceSubscriptions;
        private readonly ICosmosDbService<DataModels.RecognizedCurrency> _dbServiceCurrencies;

        public SubscriptionsController(
            ICosmosDbService<DataModels.SubscriptionForExchangeRate> dbServiceSubscriptions,
            ICosmosDbService<DataModels.RecognizedCurrency> dbServiceCurrencies)
        {
            this._dbServiceSubscriptions = dbServiceSubscriptions;
            this._dbServiceCurrencies = dbServiceCurrencies;
        }

        private async Task<IEnumerable<DataModels.RecognizedCurrency>> GetAllRecognizedCurrenciesAsync()
        {
            return await _dbServiceCurrencies.QueryAsync(cosmos => cosmos.Select(item => item));
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index(string emailFilter)
        {
            var getAllCurrencies = GetAllRecognizedCurrenciesAsync();

            if (string.IsNullOrWhiteSpace(emailFilter))
            {
                var allItems =
                    await _dbServiceSubscriptions.QueryAsync(cosmos => cosmos.Select(item => item));
                return View(
                    allItems.Select(item => new Models.SubscriptionViewModel(item, getAllCurrencies.Result))
                );
            }
            else
            {
                var filteredItems = await _dbServiceSubscriptions.QueryAsync(cosmos =>
                    cosmos.Where(item => item.EMailAddress.Contains(emailFilter))
                        .Select(item => item)
                );
                return View(
                    filteredItems.Select(item => new Models.SubscriptionViewModel(item, getAllCurrencies.Result))
                );
            }
        }

        // GET: Subscriptions/Details/partitionKey?id
        public async Task<IActionResult> Details(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var getAllCurrencies = GetAllRecognizedCurrenciesAsync();

            DataModels.SubscriptionForExchangeRate subscription =
                await _dbServiceSubscriptions.GetItemAsync(partitionKey, id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(new Models.SubscriptionViewModel(subscription, await getAllCurrencies));
        }

        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            return View(
                new Models.SubscriptionViewModel(null, GetAllRecognizedCurrenciesAsync().Result)
            );
        }

        // POST: Subscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Label,EMailAddress,CodeCurrencyToSell,CodeCurrencyToBuy,TargetPriceOfSellingCurrency")] DataModels.SubscriptionForExchangeRate subscription)
        {
            if (ModelState.IsValid)
            {
                await _dbServiceSubscriptions.AddItemAsync(subscription);
                return RedirectToAction(nameof(Index));
            }

            return View(
                new Models.SubscriptionViewModel(subscription, await GetAllRecognizedCurrenciesAsync())
            );
        }

        // GET: Subscriptions/Edit/partitionKey?id
        public async Task<IActionResult> Edit(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var getAllCurrencies = GetAllRecognizedCurrenciesAsync();

            DataModels.SubscriptionForExchangeRate subscription =
                await _dbServiceSubscriptions.GetItemAsync(partitionKey, id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(
                new Models.SubscriptionViewModel(subscription, await getAllCurrencies)
            );
        }

        // POST: Subscriptions/Edit/partitionKey
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey,
            [Bind("Id,Label,EMailAddress,CodeCurrencyToSell,CodeCurrencyToBuy,TargetPriceOfSellingCurrency")] DataModels.SubscriptionForExchangeRate subscription)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(subscription.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _dbServiceSubscriptions.UpsertItemAsync(partitionKey, subscription);
                }
                catch (Exception)
                {
                    if (await _dbServiceSubscriptions.GetItemAsync(partitionKey, subscription.Id) == null)
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

            return View(
                new Models.SubscriptionViewModel(subscription, await GetAllRecognizedCurrenciesAsync())
            );
        }

        // GET: Subscriptions/Delete/partitionKey?id
        public async Task<IActionResult> Delete(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var getAllCurrencies = GetAllRecognizedCurrenciesAsync();

            DataModels.SubscriptionForExchangeRate subscription =
                await _dbServiceSubscriptions.GetItemAsync(partitionKey, id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(
                new Models.SubscriptionViewModel(subscription, await getAllCurrencies)
            );
        }

        // POST: Subscriptions/Delete/partitionKey?id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string id)
        {
            await _dbServiceSubscriptions.DeleteItemAsync(partitionKey, id);
            return RedirectToAction(nameof(Index));
        }

    }// end of class SubscriptionController

}//end of namespace CurrencyMonitor.Controllers
