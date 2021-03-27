using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Reusable.DataAccess;

using CurrencyMonitor.DataAccess;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    public class Functions
    {
        private readonly ICosmosDbService<SubscriptionForExchangeRate> _subscriptionService;

        private readonly ICosmosDbService<ExchangeRate> _exchangeRateService;

        public Functions(ICosmosDbService<SubscriptionForExchangeRate> subscriptionService,
                         ICosmosDbService<ExchangeRate> exchangeRateService)
        {
            _subscriptionService = subscriptionService;
            _exchangeRateService = exchangeRateService;
        }

        public void RunOnSchedule(
            [TimerTrigger("0 */10 7-18 * * 1-5")] TimerInfo timer, ILogger log)
        {
            if (timer.IsPastDue)
            {
                log.LogInformation("Timer löst die Ausführung der Funktion zu spät aus!");
            }
            log.LogInformation($"Funktion wird von dem Timer ausgeführt: {DateTime.Now}");

            var exchangeRateIdGenerator = new ExchangeRateIdGenerator();

            // mittlerweile fährt den Anbieter für Wechselkurs im Voraus hoch
            var exchangeRateProvider = new ExchangeRateProvider();

            log.LogInformation("Alle Abonnements für Wechselkurs werden abgerufen...");
            IEnumerable<SubscriptionForExchangeRate> allSubscriptions =
                _subscriptionService.QueryAsync(source => source.Select(item => item)).Result;

            // sammelt jeden einzelnen Paar von Währungen:
            ISet<ExchangePair> exchangePairs = (
                from subscription in allSubscriptions
                select new ExchangePair(subscription.CodeCurrencyToSell,
                                        subscription.CodeCurrencyToBuy)
            ).ToHashSet();
            log.LogInformation($"Insgesamt {exchangePairs.Count} Wechselkurs(e) müssen erfasst werden.");

            // holt die Wechselkurse aus dem Internet:
            var exchangeRateRetrievals = from exchange in exchangePairs
                                         group exchangeRateProvider.GetLatestRateAsync(exchange)
                                         by exchange.PrimaryCurrencyCode
                                         into exchangeRateGroup
                                         select exchangeRateGroup;

            // speichert die erhaltenen Wechselkurse in der Datenbank:
            foreach (var retrievalGroup in exchangeRateRetrievals)
            {
                log.LogInformation($"Es gibt {retrievalGroup.Count()} Wechselkurs(e) mit der Währung {retrievalGroup.Key}... ");

                Task.WaitAll(retrievalGroup.ToArray());
                log.LogInformation("Erfasst.");

                var items = retrievalGroup.Select(task => {
                    ExchangeRate exchangeRate = task.Result;
                    exchangeRate.Id = exchangeRateIdGenerator.GenerateIdFor(exchangeRate);
                    return exchangeRate;
                });
                var upsertTask = _exchangeRateService.UpsertBatchAsync(items);

                upsertTask.Wait();
                if (upsertTask.Exception != null)
                {
                    log.LogInformation(
                        $"GESCHEITERT!\n***\n{upsertTask.Exception.InnerException.Message}\n***");
                    continue;
                }

                log.LogInformation("Gespeichert :-)");
            }
        }

    }// end of class Functions

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
