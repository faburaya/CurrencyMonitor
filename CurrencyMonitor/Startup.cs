using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Reusable.DataModels;
using Reusable.DataAccess;

namespace CurrencyMonitor
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        private string DatabaseName =>
            Configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value;

        private string CosmosDbAccountEndpoint =>
            Configuration.GetSection("CosmosDb").GetSection("AccountEndpoint").Value;

        private string CosmosDbAccountKey =>
            Configuration.GetSection("CosmosDb").GetSection("AccountKey").Value;

        private string DbConnectionString =>
            $"AccountEndpoint={CosmosDbAccountEndpoint};AccountKey={CosmosDbAccountKey}";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Injiziert den Service für Zugang auf Cosmos Datenbank.
        /// </summary>
        /// <typeparam name="ItemType">Der Typ des Elements, den der Service behandelt.</typeparam>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        private async Task InjectCosmosDbService<ItemType>(IServiceCollection services)
            where ItemType : CosmosDbItem<ItemType>, IEquatable<ItemType>
        {
            var service = await CosmosDbService<ItemType>
                .InitializeCosmosClientInstanceAsync(DatabaseName, DbConnectionString);

            services.AddSingleton<ICosmosDbService<ItemType>>(service);
        }

        /// <summary>
        /// Injiziert die Service, die für Authentifizierung notwendig sind.
        /// </summary>
        /// <param name="services">Die Sammlung, in der der Service injiziert wird.</param>
        private async Task InjectAuthenticationServices(IServiceCollection services)
        {
            await Task.Run(() =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    options.AccessDeniedPath = "/AccessDenied";
                })
                .AddCookie();
            });
        }

        /// <summary>
        /// Diese Methode wird von der Laufzeit gerufen, um Service zu injizieren.
        /// </summary>
        /// <param name="services">Die Sammlung, in der die Service injiziert werden.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            Task.WaitAll(new Task[] {
                InjectAuthenticationServices(services),
                InjectCosmosDbService<DataModels.RecognizedCurrency>(services),
                InjectCosmosDbService<DataModels.SubscriptionForExchangeRate>(services),
                InjectCosmosDbService<DataModels.ExchangeRate>(services)
            });
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{partitionKey?}");

                endpoints.MapRazorPages();
            });
        }

    }// end of class Startup

}// end of namespace CurrencyMonitor
