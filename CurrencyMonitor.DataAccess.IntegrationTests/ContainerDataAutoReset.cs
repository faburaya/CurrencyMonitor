using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CurrencyMonitor.DataAccess.IntegrationTests
{
    /// <summary>
    /// Gewährt Zugang auf die Daten im Cosmos Container
    /// und löscht sie jedes Mal wenn es verworfen wird.
    /// </summary>
    public class ContainerDataAutoReset : IDisposable
    {
        private Container Container { get; }

        public ContainerDataAutoReset(Container cosmosContainer)
        {
            this.Container = cosmosContainer;
        }

        public delegate IQueryable<TestItem> QueryCosmos(IOrderedQueryable<TestItem> cosmosQueryableSource);

        /// <summary>
        /// Fragt die Cosmos Datenbank ab, wartet auf Ergebnisse und gibt sie zurück.
        /// </summary>
        /// <param name="query">Die zu laufende LINQ-Abfrage.</param>
        /// <returns>Die Ergebnisse der Abfrage.</returns>
        public IEnumerable<TestItem> CollectResultsFromQuery(QueryCosmos query)
        {
            using var iterator =
                query(Container.GetItemLinqQueryable<TestItem>())
                .ToFeedIterator();

            var results = new List<TestItem>();
            while (iterator.HasMoreResults)
            {
                var response = iterator.ReadNextAsync().Result;
                results.AddRange(response);
            }

            return results;
        }

        private void EraseAllItemsInContainer()
        {
            var allItems = CollectResultsFromQuery(source => source.Select(item => item));
            var tasks = new List<Task>(capacity: allItems.Count());
            foreach (var item in allItems)
            {
                Task deleteAsyncTask =
                    Container.DeleteItemAsync<TestItem>(
                        item.Id, new PartitionKey(item.PartitionKeyValue));

                tasks.Add(deleteAsyncTask);
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ContainerDataAutoReset() => Dispose(false);

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // verwirf hier die verwalteten Ressourcen
            }

            EraseAllItemsInContainer();

            _disposed = true;
        }

    }// end of class ContainerDataAutoReset

}// using namespace CurrencyMonitor.DataAccess.IntegrationTests
