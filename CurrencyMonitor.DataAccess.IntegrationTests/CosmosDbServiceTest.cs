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
            Task.WaitAll((from item in expectedItems
                          select Fixture.Service.AddItemAsync(item))
                          .ToArray());

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

        private IEnumerable<TestItem> AddAndRetrieveItems(
            IList<TestItem> items, ContainerDataAutoReset cosmosDataAccess)
        {
            cosmosDataAccess.AddToContainer(items);

            var itemsAddedToContainer =
                cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));

            if (itemsAddedToContainer.Count != items.Count)
            {
                throw new Exception($"Es ist der Vorbereitung des Testszenarios nicht gelungen, dem Container einige Elemente hinzuzufügen! (Nur {itemsAddedToContainer.Count} Elemente statt {items.Count} sind dort gespeichert.)");
            }

            return itemsAddedToContainer;
        }

        [Fact]
        public void DeleteItem_WhenDistinct_IfAllDeleted_ThenNothingRemains()
        {
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();

            var itemsBeforeDeletion = AddAndRetrieveItems(new List<TestItem> {
                new TestItem { Name = "Paloma", Family = "Farah" },
                new TestItem { Name = "Andressa", Family = "Rabah" },
            }, cosmosDataAccess);

            Task.WaitAll((from item in itemsBeforeDeletion 
                          select Fixture.Service.DeleteItemAsync(item.PartitionKeyValue, item.Id))
                          .ToArray());

            var itemsAfterDeletion =
                cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));

            foreach (TestItem item in itemsBeforeDeletion)
            {
                Assert.DoesNotContain(itemsAfterDeletion,
                    remainingItem => item.IsEquivalentInStorageTo(remainingItem));
            }
            Assert.Empty(itemsAfterDeletion);
        }

        [Fact]
        public void DeleteItem_WhenSimilar_IfOneDeleted_ThenAnotherRemains()
        {
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();

            var itemsBeforeDeletion = AddAndRetrieveItems(new List<TestItem> {
                new TestItem { Name = "Paloma", Family = "Farah" },
                new TestItem { Name = "Andressa", Family = "Rabah" },
            }, cosmosDataAccess);

            // Löscht das erste Element:
            TestItem item1 = itemsBeforeDeletion.First();
            Fixture.Service.DeleteItemAsync(item1.PartitionKeyValue, item1.Id).Wait();

            var itemsAfterDeletion =
                cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));

            Assert.DoesNotContain(itemsAfterDeletion,
                remainingItem => item1.IsEquivalentInStorageTo(remainingItem));

            Assert.Equal(itemsBeforeDeletion.Count() - 1, itemsAfterDeletion.Count());

            // Löscht das zweite Element:
            TestItem item2 = itemsBeforeDeletion.Last();
            Fixture.Service.DeleteItemAsync(item2.PartitionKeyValue, item2.Id).Wait();

            itemsAfterDeletion =
                cosmosDataAccess.CollectResultsFromQuery(source => source.Select(item => item));

            Assert.DoesNotContain(itemsAfterDeletion,
                remainingItem => item2.IsEquivalentInStorageTo(remainingItem));

            Assert.Equal(itemsBeforeDeletion.Count() - 2, itemsAfterDeletion.Count());
        }

        [Fact]
        public void GetItemCount_BeforeAndAfterAddingNew()
        {
            using var cosmosDataAccess = Fixture.GetAccessToCosmosContainerData();

            Assert.Equal(0, Fixture.Service.GetItemCountAsync().Result);

            var addedItems = AddAndRetrieveItems(new List<TestItem> {
                new TestItem { Name = "Paloma", Family = "Farah" },
                new TestItem { Name = "Andressa", Family = "Rabah" },
            }, cosmosDataAccess);

            Assert.Equal(addedItems.Count(), Fixture.Service.GetItemCountAsync().Result);
        }

    }// end of class CosmosDbServiceTest

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
