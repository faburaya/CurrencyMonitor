using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Azure.Cosmos;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Implementierung für einen Diest, der Zugang auf Azure Cosmos Datenbank gewährt.
    /// </summary>
    public class CosmosDbService<ItemType> : ICosmosDbService<ItemType>
        where ItemType : DataModels.ICosmosDbItem
    {
        private readonly Container _container;

        public CosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(ItemType item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
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
