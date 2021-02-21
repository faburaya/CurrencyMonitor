using System;

using Microsoft.Azure.Cosmos;

using Xunit;

namespace CurrencyMonitor.DataAccess.IntegrationTests
{
    public class CosmosDatabaseFixture : IDisposable
    {
        private string ConnectionString => "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private string DatabaseName => "CurrencyMonitor.DataAccess.IntegrationTests";

        private CosmosClient Client { get; }

        public CosmosDbService<TestItem> Service { get; }

        public CosmosDatabaseFixture()
        {
            this.Client = new CosmosClient(ConnectionString);

            CosmosDbService<TestItem> cosmosDbService =
                CosmosDbService<TestItem>.InitializeCosmosClientInstanceAsync(DatabaseName, ConnectionString)
                    .GetAwaiter()
                    .GetResult();

            Assert.NotNull(cosmosDbService);
            this.Service = cosmosDbService;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CosmosDatabaseFixture() => Dispose(false);

        private bool _disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Macht alle Änderungen in Cosmos Datenbank rückgängig:
            Database database = Client.GetDatabase(DatabaseName);
            database?.DeleteAsync().Wait();

            if (disposing)
            {
                Client.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Gibt das Container der Cosmos Datenbank an.
        /// (Vorausgesetzt, dass die Container und Datenbank vorhanden sind.)
        /// </summary>
        /// <returns>Ein Objekt für das Container der Cosmos Datenbank.</returns>
        public Container GetCosmosContainer()
        {
            Container container = Client.GetContainer(DatabaseName,
                DataModels.CosmosDbPartitionedItem<TestItem>.ContainerName);
            Assert.NotNull(container);
            return container;
        }

    }// end of class CosmosDatabaseFixture

}// end of namespace CurrencyMonitor.DataAccess.IntegrationTests
