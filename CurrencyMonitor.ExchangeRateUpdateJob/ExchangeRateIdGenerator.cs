namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    internal class ExchangeRateIdGenerator
        : Reusable.DataAccess.Common.UidGenerator<DataModels.ExchangeRate>
    {
        public override string GenerateIdFor(DataModels.ExchangeRate obj)
        {
            /* Derzeit gilt ExchangeRate.PrimaryCurrencyCode als Partitionsschlüssel.
             * Damit ein bestimmter Wechselkurs einzigartig innerhalb der Datenbank besteht,
             * sucht man ExchangeRate.SecondaryCurrencyCode als ID aus. */
            return obj.SecondaryCurrencyCode;
        }
    }
}
