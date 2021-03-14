using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommandLine;
using CurrencyMonitor.DataModels;
using CurrencyMonitor.DataAccess;
using Microsoft.Azure.Cosmos;
using Reusable.DataAccess;

namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    /// <summary>
    /// Erfasst alle Wechselkurse der Abonnements aus dem Internet
    /// und speichert sie in der Datenbank.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs =
                Parser.Default.ParseArguments<CommandLineOptions>(args) as Parsed<CommandLineOptions>;

            if (parsedArgs == null)
                return;

            Console.WriteLine("Job wird gestartet...");

            // fährt den Dienst für Abonnements im Voraus und wartet darauf...
            var createSubscriptionServiceTask = CosmosDbService<SubscriptionForExchangeRate>.InitializeCosmosClientInstanceAsync(parsedArgs.Value.DatabaseName, parsedArgs.Value.ConnectionString);

            var exchangeRateIdGenerator = new ExchangeRateIdGenerator();

            // mittlerweile fährt den Dienst für Wechselkurs im Voraus hoch
            var createExchangeRateServiceTask = CosmosDbService<ExchangeRate>.InitializeCosmosClientInstanceAsync(
                parsedArgs.Value.DatabaseName,
                parsedArgs.Value.ConnectionString,
                exchangeRateIdGenerator);

            // mittlerweile fährt den Anbieter für Wechselkurs im Voraus hoch
            var exchangeRateProvider = new ExchangeRateProvider();

            using ICosmosDbService<SubscriptionForExchangeRate> subscriptionService = createSubscriptionServiceTask.Result;

            Console.WriteLine("Alle Abonnements für Wechselkurs werden abgerufen...");
            IEnumerable<SubscriptionForExchangeRate> allSubscriptions =
                subscriptionService.QueryAsync(source => source.Select(item => item)).Result;

            // sammelt jeden einzelnen Paar von Währungen:
            ISet<ExchangePair> exchangePairs = (
                from subscription in allSubscriptions
                select new ExchangePair(subscription.CodeCurrencyToSell,
                                        subscription.CodeCurrencyToBuy)
            ).ToHashSet();
            Console.WriteLine($"Insgesamt {exchangePairs.Count} Wechselkurs(e) müssen erfasst werden.");

            // holt die Wechselkurse aus dem Internet:
            var exchangeRateRetrievals = from exchange in exchangePairs
                                         group exchangeRateProvider.GetLatestRateAsync(exchange)
                                         by exchange.PrimaryCurrencyCode
                                         into exchangeRateGroup
                                         select exchangeRateGroup;

            // speichert die erhaltenen Wechselkurse in der Datenbank:
            using ICosmosDbService<ExchangeRate> exchangeRateService = createExchangeRateServiceTask.Result;
            foreach (var retrievalGroup in exchangeRateRetrievals)
            {
                Console.Write($"Es gibt {retrievalGroup.Count()} Wechselkurs(e) mit der Währung {retrievalGroup.Key}... ");

                Task.WaitAll(retrievalGroup.ToArray());
                Console.Write("Erfasst... ");

                var items = retrievalGroup.Select(task => {
                    ExchangeRate exchangeRate = task.Result;
                    exchangeRate.Id = exchangeRateIdGenerator.GenerateIdFor(exchangeRate);
                    return exchangeRate;
                });
                var upsertTask = exchangeRateService.UpsertBatchAsync(items);

                upsertTask.Wait();
                if (upsertTask.Exception != null)
                {
                    Console.WriteLine($"GESCHEITERT! {upsertTask.Exception.InnerException.Message}");
                    continue;
                }

                Console.WriteLine("Gespeichert :-)");
            }
        }

    }// end of class Program

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
