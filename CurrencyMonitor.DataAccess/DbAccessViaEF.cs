using System.Linq;

namespace CurrencyMonitor.DataAccess
{
    public class DbAccessViaEF : IDatabaseAccess
    {
        private CurrencyMonitorContext DatabaseContext { get; set; }

        public DbAccessViaEF(CurrencyMonitorContext dbContext)
        {
            this.DatabaseContext = dbContext;
        }

        public bool HasAny<Type>()
        {
            if (typeof(Type) == typeof(DataModels.RecognizedCurrency))
            {
                return DatabaseContext.RecognizedCurrency.Any();
            }
            else if (typeof(Type) == typeof(DataModels.SubscriptionForExchangeRate))
            {
                return DatabaseContext.SubscriptionForExchangeRate.Any();
            }

            return false;
        }

        public void Insert(DataModels.RecognizedCurrency obj) =>
            DatabaseContext.RecognizedCurrency.Add(obj);

        public void Insert(DataModels.SubscriptionForExchangeRate obj) =>
            DatabaseContext.SubscriptionForExchangeRate.Add(obj);

        public void Commit() => DatabaseContext.SaveChanges();
    }
}
