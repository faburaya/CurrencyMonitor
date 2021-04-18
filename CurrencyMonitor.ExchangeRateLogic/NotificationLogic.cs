using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Reusable.DataAccess;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic
{
    /// <summary>
    /// Implementiert die Logik für Benachrichtigung der Abonnenten.
    /// </summary>
    public class NotificationLogic
    {
        private readonly ICosmosDbService<SubscriptionForExchangeRate> _subscriptionService;
        private readonly ISubscriberNotifier _subscriberNotifier;

        public NotificationLogic(ICosmosDbService<SubscriptionForExchangeRate> subscriptionService,
                                 ISubscriberNotifier subscriberNotifier)
        {
            _subscriptionService = subscriptionService;
            _subscriberNotifier = subscriberNotifier;
        }

        /// <summary>
        /// Überprüft welche Abonnements werden von der Änderung des gegebenen Wechselkurses
        /// betroffen und benachrichtigt die Abonnenten.
        /// </summary>
        /// <param name="exchangeRate">Der veränderte Wechselkurs.</param>
        /// <param name="log">Gewährt Protokollierung.</param>
        /// <returns>Diese Aufgabe, die asynchron läuft.</returns>
        public Task VerifyExchangeRateAgainstSubscriptionsAndNotify(ExchangeRate exchangeRate, ILogger log)
        {
            return _subscriptionService.QueryAsync(source =>
                source.Where(item =>
                    item.CodeCurrencyToBuy == exchangeRate.PrimaryCurrencyCode
                        || item.CodeCurrencyToBuy == exchangeRate.SecondaryCurrencyCode)
                .Select(item => item)
            )
            .ContinueWith(antecendent => {
                IEnumerable<SubscriptionForExchangeRate> subscriptions = antecendent.Result;
                var subscriptionsToNotify = from item in subscriptions
                                            where NeedsNotification(item, exchangeRate)
                                            select item;

                foreach (SubscriptionForExchangeRate subscription in subscriptionsToNotify)
                {
                    log.LogInformation($"Muss {subscription.EMailAddress} Bescheid geben.");
                    _subscriberNotifier.Notify(subscription, exchangeRate);
                }
            });
        }

        /// <summary>
        /// Erkennt, wenn einem Abonnent wegen der Änderung des Wechselkurses
        /// Bescheid gegeben werden muss.
        /// </summary>
        /// <param name="subscription">Das Abonnenment.</param>
        /// <param name="exchangeRate">Der Wechselkurs.</param>
        /// <returns>Ob dem Abonnent Bescheid gegeben werden muss.</returns>
        private static bool NeedsNotification(SubscriptionForExchangeRate subscription,
                                              ExchangeRate exchangeRate)
        {
            if (subscription.CodeCurrencyToBuy == exchangeRate.PrimaryCurrencyCode
                && subscription.CodeCurrencyToSell == exchangeRate.SecondaryCurrencyCode)
            {
                decimal actualPriceToPay = new decimal(exchangeRate.PriceOfPrimaryCurrency);
                decimal targetPriceToPay = (decimal)1.0 / subscription.TargetPriceOfSellingCurrency;
                return actualPriceToPay <= targetPriceToPay
                    && AreDifferentDays(exchangeRate.Timestamp, subscription.LastNotification);
            }
            else if (subscription.CodeCurrencyToSell == exchangeRate.PrimaryCurrencyCode
                  && subscription.CodeCurrencyToBuy == exchangeRate.SecondaryCurrencyCode)
            {
                decimal actualSellingValue = new decimal(exchangeRate.PriceOfPrimaryCurrency);
                decimal targetSellingValue = subscription.TargetPriceOfSellingCurrency;
                return actualSellingValue >= targetSellingValue
                    && AreDifferentDays(exchangeRate.Timestamp, subscription.LastNotification);
            }
            else
            {
                throw new ArgumentException($"Abonnement {subscription.Id} für {subscription.CodeCurrencyToSell}-> {subscription.CodeCurrencyToBuy} und Wechselkurs {exchangeRate.PrimaryCurrencyCode}<->{exchangeRate.SecondaryCurrencyCode} stimmen nicht überein!");
            }
        }

        private static bool AreDifferentDays(DateTime a, DateTime b)
        {
            return a.Year != b.Year || a.DayOfYear != b.DayOfYear;
        }

    }// end of class NotificationLogic

}// end of namespace ExchangeRateLogic
