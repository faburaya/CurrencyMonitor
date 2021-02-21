using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

using Xunit;

namespace CurrencyMonitor.DataAccess.IntegrationTests
{
    public class CosmosDbServiceTest : IClassFixture<CosmosDatabaseFixture>
    {
        private CosmosDatabaseFixture Fixture { get; }

        public CosmosDbServiceTest(CosmosDatabaseFixture fixture)
        {
            this.Fixture = fixture;
        }

        private IEnumerable<TestItem> CollectResultsFromCosmos(IQueryable<TestItem> query)
        {
            using FeedIterator<TestItem> iterator = query.ToFeedIterator<TestItem>();

            var results = new List<TestItem>();
            while (iterator.HasMoreResults)
            {
                FeedResponse<TestItem> response = iterator.ReadNextAsync().Result;
                results.AddRange(response);
            }

            return results;
        }

        private IEnumerable<TestItem> GetAllItemsDirectlyFromCosmos()
        {
            var container = Fixture.GetCosmosContainer();
            return CollectResultsFromCosmos(
                container.GetItemLinqQueryable<TestItem>().Select(item => item));
        }

        [Fact]
        public void AddSingleItem()
        {
            var expectedItem = new TestItem { Name = "Werner", Family = "Heisenberg" };
            Fixture.Service.AddItemAsync(expectedItem).Wait();

            // Überprüfung:
            var container = Fixture.GetCosmosContainer();
            var results = CollectResultsFromCosmos(
                container.GetItemLinqQueryable<TestItem>()
                    .Where(item => item.Name == expectedItem.Name && item.Family == expectedItem.Family)
                    .Select(item => item));

            Assert.Single(results);
            TestItem actualItem = results.First();
            Assert.NotNull(actualItem);
            Assert.False(string.IsNullOrEmpty(actualItem.Id));
        }

        [Fact]
        public void AddDistinctItems()
        {
            var expectedItems = new List<TestItem> {
                new TestItem { Name = "Werner", Family = "Heisenberg" },
                new TestItem { Name = "Katze", Family = "Schrödinger" }
            };

            var tasks = new Task[expectedItems.Count];
            for (int idx = 0; idx < expectedItems.Count; ++idx)
            {
                tasks[idx] = Fixture.Service.AddItemAsync(expectedItems[idx]);
            }
            Task.WaitAll(tasks);

            // Überprüfung:
            var results = GetAllItemsDirectlyFromCosmos();
            Assert.Equal(expectedItems.Count, results.Count());

            foreach (TestItem expectedItem in expectedItems)
            {
                Assert.Contains(results,
                    actualItem => {
                        return actualItem.Name == expectedItem.Name
                            && actualItem.Family == expectedItem.Family;
                    });
            }
        }

    }// end of class CosmosDbServiceTest

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
