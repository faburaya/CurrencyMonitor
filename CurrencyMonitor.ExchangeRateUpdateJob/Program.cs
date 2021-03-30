using System;
using System.Threading.Tasks;

using CurrencyMonitor.DataModels;
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
                .ConfigureWebJobs(jobsBuilder =>
                {
                    jobsBuilder.AddAzureStorageCoreServices();
                    jobsBuilder.AddTimers();
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();

                    // Wenn ein Schlüssel in den Einstellungen vorhanden ist,
                    // macht sie zunutze, um Application Insights einzuschalten:
                    string instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    if (!string.IsNullOrEmpty(instrumentationKey))
                    {
                        loggingBuilder.AddApplicationInsightsWebJobs(
                            options => options.InstrumentationKey = instrumentationKey);
                    }

                    loggingBuilder.AddFilter(delegate (LogLevel level)
                    {
                        return level >= LogLevel.Trace;
                    });
                })
                .ConfigureServices((context, serviceCollection) =>
                {
                    Task.WaitAll(new Task[] {
                        InjectCosmosDbService<SubscriptionForExchangeRate>(serviceCollection,
                                                                           context.Configuration),
                        InjectCosmosDbService<ExchangeRate>(serviceCollection,
                                                            context.Configuration)
                    });
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
        private static async Task InjectCosmosDbService<ItemType>(IServiceCollection services,
                                                                  IConfiguration configuration)
            where ItemType : CosmosDbItem<ItemType>, IEquatable<ItemType>
        {
            string databaseName = configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
            string connectionString = configuration.GetConnectionString("CurrencyMonitorCosmos");

            var service = await CosmosDbService<ItemType>
                .InitializeCosmosClientInstanceAsync(databaseName, connectionString);

            services.AddSingleton<ICosmosDbService<ItemType>>(service);
        }

    }// end of class Program

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
