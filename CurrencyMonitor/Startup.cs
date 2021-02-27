using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        /// Der geheime Teil der Verbindungszeichenfolge kommt aus einer versteckten XML Datei.
        /// Das Geheim kann entweder als Verbindungszeichenfolge genutzt werden
        /// oder die Verbindungszeichenfolge aus den Einstellungen der Anwendung ergänzen.
        /// </summary>
        /// <param name="connectionName">Der Name der Verbindung.</param>
        /// <returns>Die Verbindugszeichenfolge für die Datenbank im Einsatz.</returns>
        private string GetSecretConnectionString(string connectionName)
        {
            var secretLoader = new DataAccess.SecretLoader(
                new DataAccess.XmlMetadata(
                    "http://www.currencymonitor.com/secrets",
                    System.IO.Path.Combine("Data", "secrets.xml"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.xsd"))
            );
            string secret = secretLoader.GetDatabaseConnString(connectionName);
            string connectionString = Configuration.GetConnectionString(connectionName);

            if (string.IsNullOrEmpty(connectionString))
            {
                return secret;
            }

            return connectionString.Replace("verborgen???", secret);
        }

        /// <summary>
        /// Injiziert den Service für Zugang auf Cosmos Datenbank.
        /// </summary>
        /// <typeparam name="ItemType">Der Typ des Elements, den der Service behandelt.</typeparam>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        private void InjectCosmosDbService<ItemType>(IServiceCollection services)
            where ItemType : DataModels.CosmosDbItem
        {
            string databaseName = Configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
            string connectionString = GetSecretConnectionString("CurrencyMonitorCosmos");

            services.AddSingleton<DataAccess.ICosmosDbService<ItemType>>(
                DataAccess.CosmosDbService<ItemType>
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
