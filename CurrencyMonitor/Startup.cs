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
        private readonly DataAccess.ConnectionStringProvider _connStringProvider;

        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _connStringProvider =
                new DataAccess.ConnectionStringProvider("secrets.xml", configuration);
        }

        /// <summary>
        /// Injiziert den Service für Zugang auf Cosmos Datenbank.
        /// </summary>
        /// <typeparam name="ItemType">Der Typ des Elements, den der Service behandelt.</typeparam>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        private void InjectCosmosDbService<ItemType>(IServiceCollection services)
            where ItemType : CosmosDbItem<ItemType>, IEquatable<ItemType>
        {
            string databaseName = _configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;
            string connectionString = _connStringProvider.GetSecretConnectionString("CurrencyMonitorCosmos");

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
            InjectCosmosDbService<DataModels.ExchangeRate>(services);
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
