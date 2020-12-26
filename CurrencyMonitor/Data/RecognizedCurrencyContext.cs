using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.Data
{
    public class RecognizedCurrencyContext : DbContext
    {
        public RecognizedCurrencyContext (DbContextOptions<RecognizedCurrencyContext> options)
            : base(options)
        {
        }

        public DbSet<CurrencyMonitor.DataModels.RecognizedCurrency> RecognizedCurrency { get; set; }
    }
}
