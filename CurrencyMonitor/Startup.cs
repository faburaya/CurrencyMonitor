using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Reusable.DataModels;
using Reusable.DataAccess;

namespace CurrencyMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Ersetzt die herkömmliche Beschaffung der Verbindungszeichenfolge.
        /// Die geheime Verbindungszeichenfolge kommt aus einer versteckten XML Datei.
        /// Wenn solche Datei nicht vorhanden ist, holt es die Verbindungszeichenfolge
        /// aus den Einstellungen der Anwendung heraus.
        /// </summary>
        /// <param name="connectionName">Der Name der Verbindung.</param>
        /// <returns>
        /// Die Verbindugszeichenfolge für die Datenbank im Einsatz, wenn vorhanden, andernfalls <c>null</c>.
        /// </returns>
        private string GetSecretConnectionString(string connectionName)
        {
            SecretLoader secretLoader = null;
            string secretFilePath = Path.Combine("Data", "secrets.xml");
            if (File.Exists(secretFilePath))
            {
                secretLoader = new SecretLoader(
                    new XmlMetadata(
                        "http://dataaccess.reusable.faburaya.com/secrets",
                        secretFilePath,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schema", "secrets.xsd"))
                );
            }

            // versucht vorzugsweise die Verbindungszeichenkette aus den Geheimnissen herauszuholen:
            string secret = secretLoader?.GetDatabaseConnString(connectionName);
            if (secret != null)
            {
                return secret;
            }

            // ... andernfalls greift auf die herkömmliche Einstellungen zurück:
            return Configuration.GetConnectionString(connectionName);
            
        }

        /// <summary>
        /// Injiziert den Service für Zugang auf Cosmos Datenbank.
        /// </summary>
        /// <typeparam name="ItemType">Der Typ des Elements, den der Service behandelt.</typeparam>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        private void InjectCosmosDbService<ItemType>(IServiceCollection services)
            where ItemType : CosmosDbItem
        {
            string databaseName = Configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
            string connectionString = GetSecretConnectionString("CurrencyMonitorCosmos");

            services.AddSingleton<ICosmosDbService<ItemType>>(
                CosmosDbService<ItemType>
                    .InitializeCosmosClientInstanceAsync(databaseName, connectionString)
                    .GetAwaiter()
                    .GetResult()
            );
        }

        /// <summary>
        /// Diese Methode wird von der Laufzeit gerufen, um Service zu injizieren.
        /// </summary>
        /// <param name="services">Die Sammlung, in der die Service injiziert werden.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            InjectCosmosDbService<DataModels.RecognizedCurrency>(services);
            InjectCosmosDbService<DataModels.SubscriptionForExchangeRate>(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{partitionKey?}");
            });
        }

    }// end of class Startup

}// end of namespace CurrencyMonitor
