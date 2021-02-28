using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Reusable.DataAccess;

namespace CurrencyMonitor.Controllers
{
    public class CurrenciesController : Controller
    {
        private readonly ICosmosDbService<DataModels.RecognizedCurrency> _dbService;

        public CurrenciesController(ICosmosDbService<DataModels.RecognizedCurrency> dbService)
        {
            this._dbService = dbService;
        }

        // GET: Currencies
        public async Task<IActionResult> Index(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(await _dbService.QueryAsync(cosmos => cosmos.Select(item => item)));
            }
            else
            {
                return View(await _dbService.QueryAsync(cosmos =>
                    cosmos.Where(item =>
                        item.Code.Contains(searchString)
                        || item.Name.Contains(searchString)
                        || item.Symbol.Contains(searchString)
                        || item.Country.Contains(searchString))
                    .OrderBy(item => item.Code)
                    .Select(item => item))
                );
            }
        }

        // GET: Currencies/Details/partitionKey?id
        public async Task<IActionResult> Details(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var recognizedCurrency = await _dbService.GetItemAsync(partitionKey, id);
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
        public async Task<IActionResult> Create(
            [Bind("Id,Code,Name,Symbol,Country")] DataModels.RecognizedCurrency recognizedCurrency)
        {
            if (ModelState.IsValid)
            {
                await _dbService.AddItemAsync(recognizedCurrency);
                return RedirectToAction(nameof(Index));
            }

            return View(recognizedCurrency);
        }

        // GET: Currencies/Edit/partitionKey?id
        public async Task<IActionResult> Edit(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var recognizedCurrency = await _dbService.GetItemAsync(partitionKey, id);
            if (recognizedCurrency == null)
            {
                return NotFound();
            }

            return View(recognizedCurrency);
        }

        // POST: Currencies/Edit/partitionKey
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey,
            [Bind("Id,Code,Name,Symbol,Country")] DataModels.RecognizedCurrency recognizedCurrency)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(recognizedCurrency.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _dbService.UpsertItemAsync(partitionKey, recognizedCurrency);
                }
                catch (Exception)
                {
                    if (await _dbService.GetItemAsync(partitionKey, recognizedCurrency.Id) == null)
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

        // GET: Currencies/Delete/partitionKey?id
        public async Task<IActionResult> Delete(string partitionKey, string id)
        {
            if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var recognizedCurrency = await _dbService.GetItemAsync(partitionKey, id);
            if (recognizedCurrency == null)
            {
                return NotFound();
            }

            return View(recognizedCurrency);
        }

        // POST: Currencies/Delete/partitionKey?id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string id)
        {
            await _dbService.DeleteItemAsync(partitionKey, id);
            return RedirectToAction(nameof(Index));
        }

    } // end of class CurrenciesController

}// end of namespace CurrencyMonitor.Controllers
