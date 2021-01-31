using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;

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
        /// <param name="containerName">Der Name des Containers.</param>
        /// <param name="connectionString">The Verbindungszeichenfolge für die Datenbank.</param>
        /// <returns>Die erstellte Instanz von <see cref="CosmosDbService{ItemType}"/></returns>
        public static async Task<CosmosDbService<ItemType>> InitializeCosmosClientInstanceAsync(
            string databaseName,
            string containerName,
            string connectionString)
        {
            var client = new CosmosClient(connectionString);
            var service = new CosmosDbService<ItemType>(client, databaseName, containerName);
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await response.Database.CreateContainerIfNotExistsAsync(
                containerName, CosmosDbPartitionedItem<ItemType>.PartitionKeyPath);

            return service;
        }

        private CosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            _container = dbClient.GetContainer(databaseName, containerName);
        }

        private static string FormatID(int id) => id.ToString("X");

        /// <summary>
        /// Fügt ein neues Element in der Datenbank hinzu.
        /// </summary>
        /// <param name="item">Das zu speichernde Element.</param>
        public async Task AddItemAsync(ItemType item)
        {
            item.Id = CosmosDbPartitionedItem<ItemType>.GenerateIdFor(item);
            await _container.CreateItemAsync(item,
                new PartitionKey(new CosmosDbPartitionedItem<ItemType>(item).PartitionKeyValue)
            );
        }

        /// <summary>
        /// Löscht ein Element in der Datenbank.
        /// </summary>
        /// <param name="partitionKey">Der Partitionsschlüssel.</param>
        /// <param name="id">Die Identifikation des Elements.</param>
        public async Task DeleteItemAsync(string partitionKey, int id)
        {
            await _container.DeleteItemAsync<ItemType>(FormatID(id), new PartitionKey(partitionKey));
        }

        /// <summary>
        /// Holt ein Element in der Datenbank.
        /// </summary>
        /// <param name="partitionKey">Der Partitionsschlüssel.</param>
        /// <param name="id">Die Identifikation des Elements.</param>
        public async Task<ItemType> GetItemAsync(string partitionKey, int id)
        {
            try
            {
                ItemResponse<ItemType> response =
                    await _container.ReadItemAsync<ItemType>(FormatID(id), new PartitionKey(partitionKey));
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
            using var query = _container.GetItemQueryIterator<int>(
                "select value count(1) from c",
                requestOptions: new QueryRequestOptions { MaxItemCount = -1 }
            );

            if (!query.HasMoreResults)
            {
                throw new ApplicationException("Datenbankabfrage für Anzahl von Elementen hat ein leeres Ergebnis zurückgegeben!");
            }

            var response = await query.ReadNextAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.First();
            }

            throw new ApplicationException("Datenbankabfrage für Anzahl von Elementen ist gescheitert!");
        }

        /// <summary>
        /// Fragt die Datenbank ab.
        /// </summary>
        /// <param name="queryString">Die SQL-Abfrage.</param>
        /// <returns>Die von der Abfrage zurückgegebenen Elemente.</returns>
        public async Task<IEnumerable<ItemType>> QueryAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<ItemType>(new QueryDefinition(queryString));
            List<ItemType> results = new List<ItemType>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException("Datenbankabfrage ist gescheitert!");
                }

                results.AddRange(response.ToList());
            }

            return results;
        }

        /// <summary>
        /// Ändert ein vorherig bestehendes Element.
        /// </summary>
        /// <param name="item">Das zu ändernde Element.</param>
        public async Task UpdateItemAsync(ItemType item)
        {
            await _container.UpsertItemAsync(item,
                new PartitionKey(new CosmosDbPartitionedItem<ItemType>(item).PartitionKeyValue)
            );
        }

    }// end of class CosmosDbService

}// end of namespace CurrencyMonitor.DataAccess
