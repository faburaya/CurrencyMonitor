using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CurrencyMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                try
                {
                    var dbService = serviceProvider.GetRequiredService<DataAccess.ICosmosDbService<DataModels.RecognizedCurrency>>();

                    var xmlDataLoader = new DataAccess.XmlDataLoader(
                        new DataAccess.XmlMetadata(
                            "http://www.currencymonitor.com/deployment",
                            System.IO.Path.Combine("Data", "deployment.xml"),
                            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployment.xsd"))
                    );
                    xmlDataLoader.Load(
                        new DataAccess.CosmosDbItemAccess<DataModels.RecognizedCurrency>(dbService)
                    );
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "W�hrend der ersten Ausf�llung der Datenbank ist ein Fehler aufgetreten!");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
