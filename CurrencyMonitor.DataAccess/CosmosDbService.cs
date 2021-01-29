using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Implementierung für einen Diest, der Zugang auf Azure Cosmos Datenbank gewährt.
    /// </summary>
    public class CosmosDbService<ItemType> : ICosmosDbService<ItemType> where ItemType : class
    {
        private readonly Container _container;

        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService<ItemType>> InitializeCosmosClientInstanceAsync(
            string databaseName,
            string containerName,
            string connectionString)
        {
            var client = new CosmosClient(connectionString);
            var service = new CosmosDbService<ItemType>(client, databaseName, containerName);
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await response.Database.CreateContainerIfNotExistsAsync(
                containerName, CosmosDbItem<ItemType>.PartitionKeyPath);

            return service;
        }

        public CosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(ItemType item)
        {
            await _container.CreateItemAsync(item,
                new PartitionKey(new CosmosDbItem<ItemType>(item).PartitionKeyValue)
            );
        }

        public async Task DeleteItemAsync(string key)
        {
            await _container.DeleteItemAsync<ItemType>(key, new PartitionKey(key));
        }

        public async Task<ItemType> GetItemAsync(string key)
        {
            try
            {
                ItemResponse<ItemType> response = await _container.ReadItemAsync<ItemType>(key, new PartitionKey(key));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }

        }

        public async Task<IEnumerable<ItemType>> QueryAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<ItemType>(new QueryDefinition(queryString));
            List<ItemType> results = new List<ItemType>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string key, ItemType item)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(key));
        }

    }// end of class CosmosDbService

}// end of namespace CurrencyMonitor.DataAccess
