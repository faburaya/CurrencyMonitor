using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

using Newtonsoft.Json;

using Xunit;

namespace CurrencyMonitor.DataAccess.IntegrationTests
{
    public class CosmosDbServiceTest
    {
        private string ConnectionString => "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private string DatabaseName => "CurrencyMonitor.DataAccess.IntegrationTests";

        /// <summary>
        /// Gibt das Container der Cosmos Datenbank an.
        /// (Vorausgesetzt, dass die Container und Datenbank vorhanden sind.)
        /// </summary>
        /// <returns>Ein Objekt für das Container der Cosmos Datenbank.</returns>
        private Container GetCosmosContainer()
        {
            var client = new CosmosClient(ConnectionString);
            Container container = client.GetContainer(DatabaseName,
                DataModels.CosmosDbPartitionedItem<Item>.ContainerName);
            Assert.NotNull(container);
            return container;
        }

        /// <summary>
        /// Macht alle Änderungen in Cosmos Datenbank rückgängig.
        /// </summary>
        private void ResetCosmos()
        {
            var client = new CosmosClient(ConnectionString);

            Database database = client.GetDatabase(DatabaseName);
            if (database != null)
            {
                database.DeleteAsync().Wait();
            }
        }

        private CosmosDbService<Item> CreateService()
        {
            CosmosDbService<Item> cosmosDbService =
                CosmosDbService<Item>.InitializeCosmosClientInstanceAsync(DatabaseName, ConnectionString)
                    .GetAwaiter()
                    .GetResult();

            Assert.NotNull(cosmosDbService);
            return cosmosDbService;
        }

        IEnumerable<Item> CollectResultsFromCosmos(IQueryable<Item> query)
        {
            using FeedIterator<Item> iterator = query.ToFeedIterator<Item>();

            var results = new List<Item>();
            while (iterator.HasMoreResults)
            {
                FeedResponse<Item> response = iterator.ReadNextAsync().Result;
                results.AddRange(response);
            }

            return results;
        }

        [Fact]
        public void InitializeCosmosClientInstance()
        {
            try
            {
                CreateService();
            }
            finally
            {
                ResetCosmos();
            }
        }

        [Fact]
        public void AddSingleItem()
        {
            try
            {
                CosmosDbService<Item> service = CreateService();
                var expectedItem = new Item { Name = "Werner", Family = "Heisenberg" };
                service.AddItemAsync(expectedItem).Wait();

                // Überprüfung:
                var container = GetCosmosContainer();
                var actualItem = CollectResultsFromCosmos(
                    container.GetItemLinqQueryable<Item>()
                        .Where(item => item.Name == expectedItem.Name && item.Family == expectedItem.Family)
                        .Select(item => item)
                    ).FirstOrDefault();

                Assert.NotNull(actualItem);
                Assert.False(string.IsNullOrEmpty(actualItem.Id));
            }
            finally
            {
                ResetCosmos();
            }
        }

    }// end of class CosmosDbServiceTest

    /// <summary>
    /// Dieser Type sollte als Element der Datenbank gut funktionieren.
    /// </summary>
    [DataModels.CosmosContainer(Name = "CosmosDbServiceTest.Item")]
    internal class Item : DataModels.CosmosDbItem
    {
        [Required]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        public override string PartitionKeyValue => Family;

        [Required]
        [DataModels.CosmosPartitionKey]
        [JsonProperty(PropertyName = "family")]
        public string Family { get; set; }
    }

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
