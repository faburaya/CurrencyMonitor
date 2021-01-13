
namespace CurrencyMonitor.DataAccess
{
    public interface IDatabaseAccess
    {
        void Insert(DataModels.RecognizedCurrency obj);

        void Insert(DataModels.SubscriptionForExchangeRate obj);

        bool HasAny<Type>();

        void Commit();
    }
}
