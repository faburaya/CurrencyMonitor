using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

using CurrencyMonitor.Data;

namespace CurrencyMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private string GetSecretConnectionString(string connectionName)
        {
            var secretLoader = new SecretLoader(
                new SecretLoader.XmlMetadata(
                    "http://www.currencymonitor.com/secrets",
                    System.IO.Path.Combine("Data", "secrets.xml"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.xsd"))
            );
            string secretPrefix = secretLoader.GetDatabaseConnString(connectionName);
            return Configuration.GetConnectionString(connectionName).Replace("verborgen???", secretPrefix);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<CurrencyMonitorContext>(options =>
                    options.UseSqlServer(GetSecretConnectionString("CurrencyMonitorContext")));
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
