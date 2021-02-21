using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Implementierung für einen Diest, der Zugang auf Azure Cosmos Datenbank gewährt.
    /// </summary>
    public class CosmosDbService<ItemType>
        : ICosmosDbService<ItemType>
        where ItemType : DataModels.CosmosDbItem
    {
        private readonly Container _container;

        /// <summary>
        /// Gewährleistet, dass die Azure Cosmos Datenbank und Container vorhanden sind,
        /// dann initialisiert ein Client und gibt es der Erstellung einer Instanz von
        /// <see cref="CosmosDbService{ItemType}"/> ab.
        /// </summary>
        /// <param name="databaseName">Der Name der Datenbank.</param>
        /// <param name="connectionString">The Verbindungszeichenfolge für die Datenbank.</param>
        /// <returns>Die erstellte Instanz von <see cref="CosmosDbService{ItemType}"/></returns>
        public static async Task<CosmosDbService<ItemType>> InitializeCosmosClientInstanceAsync(string databaseName, string connectionString)
        {
            var client = new CosmosClient(connectionString);
            var service = new CosmosDbService<ItemType>(client, databaseName);
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await response.Database.CreateContainerIfNotExistsAsync(
                DataModels.CosmosDbPartitionedItem<ItemType>.ContainerName,
                DataModels.CosmosDbPartitionedItem<ItemType>.PartitionKeyPath);

            return service;
        }

        private CosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _container = dbClient.GetContainer(databaseName, DataModels.CosmosDbPartitionedItem<ItemType>.ContainerName);
        }

        /// <summary>
        /// Fragt die Datenbank ab.
        /// </summary>
        /// <param name="query">Die LINQ-Abfrage.</param>
        /// <returns>Die von der Abfrage zurückgegebenen Elemente.</returns>
        public async Task<IEnumerable<ItemType>> QueryAsync(
            Func<IOrderedQueryable<ItemType>, IQueryable<ItemType>> query)
        {
            var results = new List<ItemType>();

            using FeedIterator<ItemType> iterator =
                query(_container.GetItemLinqQueryable<ItemType>()).ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                FeedResponse<ItemType> response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        /// <summary>
        /// Holt ein Element in der Datenbank.
        /// </summary>
        /// <param name="partitionKey">Der Partitionsschlüssel.</param>
        /// <param name="id">Die Identifikation des Elements.</param>
        public async Task<ItemType> GetItemAsync(string partitionKey, string id)
        {
            try
            {
                ItemResponse<ItemType> response =
                    await _container.ReadItemAsync<ItemType>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        /// <summary>
        /// Stellt das Anzahl von bisherig gespeicherten Elementen.
        /// </summary>
        /// <returns>
        /// Wie viele Element des Typs in der Datenbank dastehen,
        /// oder eine negative Nummer, wenn es einen Fehler gibt.
        /// </returns>
        public async Task<int> GetItemCountAsync()
        {
            using FeedIterator<int> query = _container.GetItemQueryIterator<int>(
                "select value count(1) from c",
                requestOptions: new QueryRequestOptions { MaxItemCount = -1 }
            );

            if (!query.HasMoreResults)
            {
                throw new ApplicationException("Datenbankabfrage für Anzahl von Elementen hat ein leeres Ergebnis zurückgegeben!");
            }

            var response = await query.ReadNextAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException("Datenbankabfrage für Anzahl von Elementen ist gescheitert!");
            }

            return response.First();
        }

        /// <summary>
        /// Fügt ein neues Element in der Datenbank hinzu.
        /// </summary>
        /// <param name="item">Das zu speichernde Element.</param>
        public async Task AddItemAsync(ItemType item)
        {
            item = item.ShallowCopy<ItemType>();
            item.Id = DataModels.CosmosDbPartitionedItem<ItemType>.GenerateIdFor(item);
            await _container.CreateItemAsync(item,
                new PartitionKey(DataModels.CosmosDbPartitionedItem<ItemType>.GetPartitionKeyValue(item))
            );
        }

        /// <summary>
        /// Löscht ein Element in der Datenbank.
        /// </summary>
        /// <param name="partitionKey">Der Partitionsschlüssel.</param>
        /// <param name="id">Die Identifikation des Elements.</param>
        public async Task DeleteItemAsync(string partitionKey, string id)
        {
            await _container.DeleteItemAsync<ItemType>(id, new PartitionKey(partitionKey));
        }

        /// <summary>
        /// Ändert ein vorherig bestehendes Element.
        /// </summary>
        /// <param name="item">Das zu ändernde Element.</param>
        public async Task UpdateItemAsync(ItemType item)
        {
            await _container.UpsertItemAsync(item,
                new PartitionKey(DataModels.CosmosDbPartitionedItem<ItemType>.GetPartitionKeyValue(item))
            );
        }

    }// end of class CosmosDbService

}// end of namespace CurrencyMonitor.DataAccess
