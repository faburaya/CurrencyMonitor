using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [Fact]
        public void AddSingleItem()
        {
            var expectedItem = new TestItem { Name = "Werner", Family = "Heisenberg" };
            Fixture.Service.AddItemAsync(expectedItem).Wait();

            // Überprüfung:
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();
            var results = cosmosDataAccess.CollectResultsFromQuery(source =>
                source.Where(item => item.Name == expectedItem.Name
                    && item.Family == expectedItem.Family)
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
            AddMultipleItems(expectedItems);
        }

        [Fact]
        public void AddEquivalentItems()
        {
            var item = new TestItem { Name = "Andressa", Family = "Rabah" };
            var expectedItems = new List<TestItem> { item, item };
            AddMultipleItems(expectedItems);
        }

        private void AddMultipleItems(IList<TestItem> expectedItems)
        {
            var tasks = new Task[expectedItems.Count];
            for (int idx = 0; idx < tasks.Length; ++idx)
            {
                tasks[idx] = Fixture.Service.AddItemAsync(expectedItems[idx]);
            }
            Task.WaitAll(tasks);

            // Überprüfung:
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();
            var results = cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));
            Assert.Equal(expectedItems.Count, results.Count());

            foreach (TestItem expectedItem in expectedItems)
            {
                Assert.Contains(results,
                    actualItem => {
                        return actualItem.Name == expectedItem.Name
                            && actualItem.Family == expectedItem.Family;
                    });
            }

            Assert.All(results, item => Assert.False(string.IsNullOrEmpty(item.Id)));
        }

    }// end of class CosmosDbServiceTest

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
