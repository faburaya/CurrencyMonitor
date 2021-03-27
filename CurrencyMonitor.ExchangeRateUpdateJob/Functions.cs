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
            // mittlerweile fährt den Anbieter für Wechselkurs im Voraus hoch
            var exchangeRateProvider = new ExchangeRateProvider();

            log.LogTrace("Alle Abonnements für Wechselkurs werden abgerufen...");
            IEnumerable<SubscriptionForExchangeRate> allSubscriptions =
                _subscriptionService.QueryAsync(source => source.Select(item => item)).Result;

            // sammelt jeden einzelnen Paar von Währungen:
            ISet<ExchangePair> exchangePairs = (
                from subscription in allSubscriptions
                select new ExchangePair(subscription.CodeCurrencyToSell,
                                        subscription.CodeCurrencyToBuy)
            ).ToHashSet();
            log.LogTrace($"Insgesamt {exchangePairs.Count} Wechselkurs(e) müssen erfasst werden.");

            // holt die Wechselkurse aus dem Internet:
            var exchangeRateRetrievals = from exchange in exchangePairs
                                         group exchangeRateProvider.GetLatestRateAsync(exchange)
                                         by exchange.PrimaryCurrencyCode
                                         into exchangeRateGroup
                                         select exchangeRateGroup;

            var exchangeRateIdGenerator = new ExchangeRateIdGenerator();

            // speichert die erhaltenen Wechselkurse in der Datenbank:
            foreach (var retrievalGroup in exchangeRateRetrievals)
            {
                log.LogTrace($"Mit der Währung {retrievalGroup.Key} werden {retrievalGroup.Count()} Wechselkurse erfasst und in der Datenbank gespeichert...");

                Task.WaitAll(retrievalGroup.ToArray());

                var items = retrievalGroup.Select(task => {
                    ExchangeRate exchangeRate = task.Result;
                    exchangeRate.Id = exchangeRateIdGenerator.GenerateIdFor(exchangeRate);
                    return exchangeRate;
                });
                var upsertTask = _exchangeRateService.UpsertBatchAsync(items);

                upsertTask.Wait();
                if (upsertTask.Exception != null)
                {
                    log.LogError(
                        $"GESCHEITERT!\n***\n{upsertTask.Exception.InnerException.Message}\n***");
                    continue;
                }
            }
        }

    }// end of class Functions

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
