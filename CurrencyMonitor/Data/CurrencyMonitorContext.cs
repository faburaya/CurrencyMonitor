using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CurrencyMonitor.Data
{
    public class CurrencyMonitorContext : DbContext
    {
        public CurrencyMonitorContext (DbContextOptions<CurrencyMonitorContext> options)
            : base(options)
        {
        }

        public DbSet<DataModels.SubscriptionForExchangeRate> SubscriptionForExchangeRate { get; set; }

        public DbSet<DataModels.RecognizedCurrency> RecognizedCurrency { get; set; }
    }
}
