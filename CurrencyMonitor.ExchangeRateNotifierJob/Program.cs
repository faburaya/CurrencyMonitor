using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reusable.DataAccess;
using CurrencyMonitor.DataModels;
using CurrencyMonitor.ExchangeRateLogic;

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

                    var cosmosDbService = CosmosDbService<SubscriptionForExchangeRate>
                        .InitializeCosmosClientInstanceAsync(databaseName, connectionString)
                        .GetAwaiter()
                        .GetResult();

                    serviceCollection.AddSingleton<ICosmosDbService<SubscriptionForExchangeRate>>(cosmosDbService);

                    var smtpRelayConfig = context.Configuration.GetSection("SmtpRelay");
                    int smtpPort = int.Parse(smtpRelayConfig.GetSection("Port").Value);
                    string smtpHost = smtpRelayConfig.GetSection("Host").Value;

                    var smtpCredential = new Reusable.Utils.PasswordBasedCredential {
                        UserId = smtpRelayConfig.GetSection("UserID").Value,
                       Password = smtpRelayConfig.GetSection("Password").Value
                    };

                    var mailerService = new SubscriberMailer(smtpHost, smtpPort, "exchange.rate.notifier@currencymonitor.de", smtpCredential);

                    serviceCollection.AddSingleton<ISubscriberNotifier>(mailerService);
                });

            using (var host = hostBuilder.Build())
            {
                await host.RunAsync();
            }
        }

    }// end of class Program

}// end of namespace CurrencyMonitor.ExchangeRateNotifierJob
