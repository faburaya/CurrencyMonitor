using System;
using System.Threading.Tasks;

using CurrencyMonitor.DataModels;
using CurrencyMonitor.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reusable.DataAccess;
using Reusable.DataModels;

namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    /// <summary>
    /// Erfasst alle Wechselkurse der Abonnements aus dem Internet
    /// und speichert sie in der Datenbank.
    /// </summary>
    class Program
    {
        static async Task Main()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder
                .ConfigureWebJobs(jobsBuilder => {
                    jobsBuilder.AddAzureStorageCoreServices();
                    jobsBuilder.AddTimers();
                })
                .ConfigureLogging((context, loggingBuilder) => {
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((context, serviceCollection) => {
                    var connStringProvider =
                        new ConnectionStringProvider("secrets.xml", context.Configuration);

                    InjectCosmosDbService<SubscriptionForExchangeRate>(
                        serviceCollection, context.Configuration, connStringProvider);

                    InjectCosmosDbService<ExchangeRate>(
                        serviceCollection, context.Configuration, connStringProvider);
                });

            using (var host = hostBuilder.Build())
            {
                await host.RunAsync();
            }
        }

        /// <summary>
        /// Injiziert den Service für Zugang auf Cosmos Datenbank.
        /// </summary>
        /// <typeparam name="ItemType">Der Typ des Elements, den der Service behandelt.</typeparam>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        /// <param name="configuration">Gewährt die Einstellungen für die Anwendung.</param>
        /// <param name="connStringProvider">Gewährt die Verbindugszeichenketten.</param>
        private static void InjectCosmosDbService<ItemType>(IServiceCollection services,
                                                            IConfiguration configuration,
                                                            ConnectionStringProvider connStringProvider)
            where ItemType : CosmosDbItem<ItemType>, IEquatable<ItemType>
        {
            string databaseName = configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
            string connectionString = connStringProvider.GetSecretConnectionString("CurrencyMonitorCosmos");

            services.AddSingleton<ICosmosDbService<ItemType>>(
                CosmosDbService<ItemType>
                    .InitializeCosmosClientInstanceAsync(databaseName, connectionString)
                    .GetAwaiter()
                    .GetResult()
            );
        }

    }// end of class Program

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
