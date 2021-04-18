using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic
{
    /// <summary>
    /// Schnittstelle für die Benachrichtigung der Abonnenten.
    /// </summary>
    public interface ISubscriberNotifier
    {
        /// <summary>
        /// Benachrichtigt einen Abonnenten, der den Wechselkurs beobachtet.
        /// </summary>
        /// <param name="subscription">Das Abonnement.</param>
        /// <param name="exchangeRate">Der beobachtende Wechselkurs.</param>
        void Notify(SubscriptionForExchangeRate subscription, ExchangeRate exchangeRate);
    }
}