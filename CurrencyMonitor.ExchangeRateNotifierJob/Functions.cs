using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reusable.DataAccess;

using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateNotifierJob
{
    /// <summary>
    /// Veröffentlicht Funktionen, die von dem Webauftrag ausgeführt werden müssen.
    /// </summary>
    public class Functions
    {
        private readonly ICosmosDbService<SubscriptionForExchangeRate> _subscriptionService;

        public Functions(ICosmosDbService<SubscriptionForExchangeRate> subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        private const int _maxParallelTasks = 40;

        /// <summary>
        /// Läuft jedes Mal wenn ein Wechselkurs sich in der Datenbank ändert
        /// und gibt den Abonnenten Bescheid, wenn es notwendig ist.
        /// </summary>
        /// <param name="input">Eine Liste von Dokumenten, die den Wechselkursen entsprechen,
        /// die sich änderten.</param>
        /// <param name="log">Gewährt Protokollierung.</param>
        public void Run([CosmosDBTrigger(
            databaseName: "CurrencyMonitorDatenbank",
            collectionName: "Wechselkurs",
            ConnectionStringSetting = "CurrencyMonitorCosmos",
            LeaseCollectionName = "VeränderteWechselkurse",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input == null)
            {
                log.LogWarning("Liste von veränderten Dokumenten ist leer!");
                return;
            }

            var parallelTasks = new TaskQueue(_maxParallelTasks);

            // für jeden Wechselkurs, der aktualisiert wurde:
            foreach (Document document in input)
            {
                ExchangeRate exchangeRate =
                    JsonConvert.DeserializeObject<ExchangeRate>(document.ToString());

                // welche Abonnements werden betroffen?
                Task task = _subscriptionService.QueryAsync(source =>
                    source.Where(item =>
                        item.CodeCurrencyToBuy == exchangeRate.PrimaryCurrencyCode
                         || item.CodeCurrencyToBuy == exchangeRate.SecondaryCurrencyCode)
                    .Select(item => item)
                ).ContinueWith(antecendent => {
                    IEnumerable<SubscriptionForExchangeRate> subscriptions = antecendent.Result;
                    var subscriptionsToNotify = from item in subscriptions
                                                where NeedsNotification(item, exchangeRate)
                                                select item;

                    foreach (SubscriptionForExchangeRate subscription in subscriptionsToNotify)
                    {
                        log.LogInformation($"Muss {subscription.EMailAddress} Bescheid geben.");
                    }
                });

                parallelTasks.Add(task);
            }
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
                decimal targetPriceToPay = subscription.TargetPriceOfSellingCurrency;
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

    }// end of class Functions

}// end of namespace CurrencyMonitor.ExchangeRateNotifierJob
