using System.Threading.Tasks;

using CurrencyMonitor.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reusable.DataAccess;

namespace CurrencyMonitor.ExchangeRateNotifierJob
{
    /// <summary>
    /// 
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
                    jobsBuilder.AddCosmosDB();
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
                    string databaseName =
                        context.Configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
                    string connectionString =
                        context.Configuration.GetConnectionString("CurrencyMonitorCosmos");

                    var service = CosmosDbService<SubscriptionForExchangeRate>
                        .InitializeCosmosClientInstanceAsync(databaseName, connectionString)
                        .GetAwaiter()
                        .GetResult();

                    serviceCollection.AddSingleton<ICosmosDbService<SubscriptionForExchangeRate>>(service);

                    // TODO: Fügen SubscriberMailer hinzu!
                });

            using (var host = hostBuilder.Build())
            {
                await host.RunAsync();
            }
        }

    }// end of class Program

}// end of namespace CurrencyMonitor.ExchangeRateNotifierJob
