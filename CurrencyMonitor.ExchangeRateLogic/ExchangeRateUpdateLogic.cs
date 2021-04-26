using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Reusable.DataAccess;

using CurrencyMonitor.DataAccess;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic
{
    /// <summary>
    /// Implementiert die Logik für das Update von Wechselkursen in der Datenbank.
    /// </summary>
    public class ExchangeRateUpdateLogic
    {
        private readonly IExchangeRateProvider _exchangeRateProvider;

        private readonly ICosmosDbService<SubscriptionForExchangeRate> _subscriptionService;

        private readonly ICosmosDbService<ExchangeRate> _exchangeRateService;

        public ExchangeRateUpdateLogic(
            IExchangeRateProvider exchangeRateProvider,
            ICosmosDbService<SubscriptionForExchangeRate> subscriptionService,
            ICosmosDbService<ExchangeRate> exchangeRateService)
        {
            _exchangeRateProvider = exchangeRateProvider;
            _subscriptionService = subscriptionService;
            _exchangeRateService = exchangeRateService;
        }

        /// <summary>
        /// Ruft die derzeitigen Wechselkurse aus dem Internet ab,
        /// die von den Abonnenten beobachtet werden, und speichert
        /// sie in der Datenbank.
        /// </summary>
        /// <param name="log">Gewährt Protokollierung.</param>
        public void FetchAndUpdateExchangeRates(ILogger log)
        {
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
                                         group _exchangeRateProvider.GetLatestRateAsync(exchange)
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

    }// end of class ExchangeRateUpdateLogic

}// end of CurrencyMonitor.ExchangeRateLogic
