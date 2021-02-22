using System;
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
        public void AddItem_WhenSingle()
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
        public void AddItem_WhenDistinct()
        {
            var expectedItems = new List<TestItem> {
                new TestItem { Name = "Werner", Family = "Heisenberg" },
                new TestItem { Name = "Katze", Family = "Schrödinger" },
            };
            TestAddMultipleItems(expectedItems);
        }

        [Fact]
        public void AddItem_WhenEquivalent()
        {
            var item = new TestItem { Name = "Andressa", Family = "Rabah" };
            var expectedItems = new List<TestItem> { item, item };
            TestAddMultipleItems(expectedItems);
        }

        private void TestAddMultipleItems(IList<TestItem> expectedItems)
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

        [Fact]
        public void DeleteItem_WhenPresent()
        {
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();

            var someItems = new List<TestItem> {
                new TestItem { Name = "Paloma", Family = "Farah" },
                new TestItem { Name = "Andressa", Family = "Rabah" },
            };
            cosmosDataAccess.AddToContainer(someItems);

            var itemsBeforeDeletion = cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));
            if (itemsBeforeDeletion.Count != someItems.Count)
            {
                throw new Exception($"Es ist der Vorbereitung des Testszenarios nicht gelungen, dem Container einige Elemente hinzuzufügen! (Nur {itemsBeforeDeletion.Count} Elemente statt {itemsBeforeDeletion.Count} sind dort gespeichert.)");
            }

            Task.WaitAll((from item in itemsBeforeDeletion 
                          select Fixture.Service.DeleteItemAsync(item.PartitionKeyValue, item.Id))
                          .ToArray());

            var itemsAfterDeletion = cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));
            foreach (TestItem item in itemsBeforeDeletion)
            {
                Assert.DoesNotContain(itemsAfterDeletion,
                    remainingItem => item.IsEquivalentInStorageTo(remainingItem));
            }
            Assert.Empty(itemsAfterDeletion);
        }

    }// end of class CosmosDbServiceTest

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
