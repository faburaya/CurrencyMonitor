using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Reusable.DataAccess;

using CurrencyMonitor.DataAccess;
using CurrencyMonitor.DataModels;
using CurrencyMonitor.ExchangeRateLogic;

namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    public class Functions : ExchangeRateUpdateLogic
    {
        public Functions(IExchangeRateProvider exchangeRateProvider,
                         ICosmosDbService<SubscriptionForExchangeRate> subscriptionService,
                         ICosmosDbService<ExchangeRate> exchangeRateService)
            : base(exchangeRateProvider, subscriptionService, exchangeRateService)
        {
        }

        public void RunOnSchedule([TimerTrigger("0 */10 7-18 * * 1-5")] TimerInfo timer, ILogger log)
        {
            FetchAndUpdateExchangeRates(log);
        }

    }// end of class Functions

}// end of namespace CurrencyMonitor.ExchangeRateUpdateJob
