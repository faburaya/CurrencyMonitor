using System.Collections.Generic;
using System.Linq;

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

        IEnumerable<TestItem> CollectResultsFromCosmos(IQueryable<TestItem> query)
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

        [Fact]
        public void AddSingleItem()
        {
            var expectedItem = new TestItem { Name = "Werner", Family = "Heisenberg" };
            Fixture.Service.AddItemAsync(expectedItem).Wait();

            // Überprüfung:
            var container = Fixture.GetCosmosContainer();
            var actualItem = CollectResultsFromCosmos(
                container.GetItemLinqQueryable<TestItem>()
                    .Where(item => item.Name == expectedItem.Name && item.Family == expectedItem.Family)
                    .Select(item => item)
                ).FirstOrDefault();

            Assert.NotNull(actualItem);
            Assert.False(string.IsNullOrEmpty(actualItem.Id));
        }

    }// end of class CosmosDbServiceTest

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
