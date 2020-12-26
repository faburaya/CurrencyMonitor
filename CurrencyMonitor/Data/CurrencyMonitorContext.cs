using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Data
{
    public class CurrencyMonitorContext : DbContext
    {
        public CurrencyMonitorContext (DbContextOptions<CurrencyMonitorContext> options)
            : base(options)
        {
        }

        public DbSet<CurrencyMonitor.DataModels.SubscriptionForExchangeRate> SubscriptionForExchangeRate { get; set; }

        public DbSet<CurrencyMonitor.DataModels.RecognizedCurrency> RecognizedCurrency { get; set; }
    }
}
