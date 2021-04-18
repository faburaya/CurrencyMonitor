using System.Collections.Generic;

using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reusable.DataAccess;
using Reusable.Utils;
using CurrencyMonitor.DataModels;
using CurrencyMonitor.ExchangeRateLogic;

namespace CurrencyMonitor.ExchangeRateNotifierJob
{
    /// <summary>
    /// Veröffentlicht Funktionen, die von dem Webauftrag ausgeführt werden müssen.
    /// </summary>
    public class Functions : NotificationLogic
    {
        public Functions(ICosmosDbService<SubscriptionForExchangeRate> subscriptionService,
                         ISubscriberNotifier subscriberNotifier)
            : base(subscriptionService, subscriberNotifier)
        {
        }

        private const int _maxParallelTasks = 40;

        /// <summary>
        /// Läuft jedes Mal wenn ein Wechselkurs sich in der Datenbank ändert
        /// und gibt den Abonnenten Bescheid, wenn es notwendig ist.
        /// </summary>
        /// <param name="input">
        /// Eine Liste von Dokumenten, die den veränderten Wechselkursen entsprechen.
        /// </param>
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

                parallelTasks.Add(
                    VerifyExchangeRateAgainstSubscriptionsAndNotify(exchangeRate, log));
            }
        }

    }// end of class Functions

}// end of namespace CurrencyMonitor.ExchangeRateNotifierJob
