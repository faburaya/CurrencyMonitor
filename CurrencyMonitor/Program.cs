using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
                    using var dbContext = new DataAccess.CurrencyMonitorContext(
                        serviceProvider.GetRequiredService<DbContextOptions<DataAccess.CurrencyMonitorContext>>());

                    var xmlDataLoader = new DataAccess.XmlDataLoader(
                        new DataAccess.XmlMetadata(
                            "http://www.currencymonitor.com/deployment",
                            System.IO.Path.Combine("Data", "deployment.xml"),
                            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deployment.xsd"))
                    );
                    xmlDataLoader.Load(
                        new DataAccess.SqlTableAccessViaEF<DataModels.RecognizedCurrency>(dbContext)
                    );
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Während der ersten Ausfüllung der Datenbank ist ein Fehler aufgetreten!");
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
