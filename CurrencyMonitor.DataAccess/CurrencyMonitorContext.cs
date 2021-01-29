﻿using Microsoft.EntityFrameworkCore;

namespace CurrencyMonitor.DataAccess
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