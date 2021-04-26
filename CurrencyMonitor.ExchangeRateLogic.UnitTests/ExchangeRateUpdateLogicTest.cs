using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Reusable.DataAccess;

using CurrencyMonitor.DataAccess;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
{
    using QuerySubscriptions = Func<IOrderedQueryable<SubscriptionForExchangeRate>, IQueryable<SubscriptionForExchangeRate>>;

    using QueryExchangeRates = Func<IOrderedQueryable<ExchangeRate>, IQueryable<ExchangeRate>>;

    public class ExchangeRateUpdateLogicTest
    {
        private readonly Mock<ILogger> _loggerMock;

        public ExchangeRateUpdateLogicTest()
        {
            _loggerMock = new Mock<ILogger>(MockBehavior.Loose);
        }

        private static Mock<ICosmosDbService<SubscriptionForExchangeRate>> CreateMockServiceReturning(
            IEnumerable<SubscriptionForExchangeRate> givenSubscriptions)
        {
            var mock = new Mock<ICosmosDbService<SubscriptionForExchangeRate>>(MockBehavior.Strict);

            mock.Setup(
                obj => obj.QueryAsync(It.IsAny<QuerySubscriptions>())
            ).ReturnsAsync(givenSubscriptions);

            return mock;
        }

        private static Mock<IExchangeRateProvider> CreateMockToProvide(
            IEnumerable<ExchangeRate> givenExchangeRates)
        {
            var mock = new Mock<IExchangeRateProvider>(MockBehavior.Strict);
            
            foreach (ExchangeRate exchangeRate in givenExchangeRates)
            {
                mock.Setup(obj =>
                    obj.GetLatestRateAsync(
                        It.Is<ExchangePair>(param =>
                            param.Equals(new ExchangePair(exchangeRate.PrimaryCurrencyCode,
                                                          exchangeRate.SecondaryCurrencyCode))))
                ).ReturnsAsync(exchangeRate);
            }

            return mock;
        }

        [Fact]
        public void FetchAndUpdateExchangeRates_WhenNoSubscription_ThenDoNothing()
        {
            Mock<IExchangeRateProvider> exchangeRateProviderMock =
                CreateMockToProvide(new ExchangeRate[] { });

            Mock<ICosmosDbService<SubscriptionForExchangeRate>> subscriptionServiceMock =
                CreateMockServiceReturning(new SubscriptionForExchangeRate[] { });

            var fakeExchangeRateService = new FakeCosmosDbService<ExchangeRate>();

            var updateLogic = new ExchangeRateUpdateLogic(exchangeRateProviderMock.Object,
                                                          subscriptionServiceMock.Object,
                                                          fakeExchangeRateService);

            updateLogic.FetchAndUpdateExchangeRates(_loggerMock.Object);

            fakeExchangeRateService.VerifyWhetherUpsertedItemsAre(new ExchangeRate[] { });
            subscriptionServiceMock.Verify();
            exchangeRateProviderMock.Verify();
        }

        [Fact]
        public void FetchAndUpdateExchangeRates_WhenManySubscriptions_IfExchangeRatesAreDistincts_ThenUpsertAll()
        {
            var subscriptions = new List<SubscriptionForExchangeRate>
            {
                new SubscriptionForExchangeRate
                {
                    Id = "CAFE",
                    Label = "Europäer kauft BRL",
                    EMailAddress = "ein@mann.de",
                    CodeCurrencyToBuy = "BRL",
                    CodeCurrencyToSell = "EUR",
                    TargetPriceOfSellingCurrency = new decimal(7.0),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Id = "BABE",
                    Label = "Amerikanerin kauft EUR",
                    EMailAddress = "eine@frau.de",
                    CodeCurrencyToBuy = "EUR",
                    CodeCurrencyToSell = "USD",
                    TargetPriceOfSellingCurrency = new decimal(1.0),
                    LastNotification = DateTime.UnixEpoch
                },
            };

            Mock<ICosmosDbService<SubscriptionForExchangeRate>> subscriptionServiceMock =
                CreateMockServiceReturning(subscriptions);

            var exchangeRates = new List<ExchangeRate>
            {
                ExchangeRate.CreateFrom(1.0, "EUR", 6.5, "BRL"),
                ExchangeRate.CreateFrom(1.0, "EUR", 1.2, "USD"),
            };

            Mock<IExchangeRateProvider> exchangeRateProviderMock = CreateMockToProvide(exchangeRates);

            var fakeExchangeRateService = new FakeCosmosDbService<ExchangeRate>();

            var updateLogic = new ExchangeRateUpdateLogic(exchangeRateProviderMock.Object,
                                                          subscriptionServiceMock.Object,
                                                          fakeExchangeRateService);

            updateLogic.FetchAndUpdateExchangeRates(_loggerMock.Object);

            var idGenerator = new ExchangeRateIdGenerator();
            exchangeRates.ForEach(exchangeRate =>
                exchangeRate.Id = idGenerator.GenerateIdFor(exchangeRate));

            fakeExchangeRateService.VerifyWhetherUpsertedItemsAre(exchangeRates);
            subscriptionServiceMock.Verify();
            exchangeRateProviderMock.Verify();
        }

        [Fact]
        public void FetchAndUpdateExchangeRates_WhenManySubscriptions_IfExchangeRatesRepeat_ThenUpsertOnlyDistinctOnes()
        {
            var subscriptions = new List<SubscriptionForExchangeRate>
            {
                new SubscriptionForExchangeRate
                {
                    Id = "CAFE",
                    Label = "Europäer kauft BRL",
                    EMailAddress = "ein@mann.de",
                    CodeCurrencyToBuy = "BRL",
                    CodeCurrencyToSell = "EUR",
                    TargetPriceOfSellingCurrency = new decimal(7.0),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Id = "FEED",
                    Label = "Brasilianer kauft EUR",
                    EMailAddress = "ich@dort.de",
                    CodeCurrencyToBuy = "EUR",
                    CodeCurrencyToSell = "BRL",
                    TargetPriceOfSellingCurrency = new decimal(0.25),
                    LastNotification = DateTime.UnixEpoch
                },
            };

            Mock<ICosmosDbService<SubscriptionForExchangeRate>> subscriptionServiceMock =
                CreateMockServiceReturning(subscriptions);

            var exchangeRates = new List<ExchangeRate>
            {
                ExchangeRate.CreateFrom(1.0, "EUR", 6.5, "BRL")
            };

            Mock<IExchangeRateProvider> exchangeRateProviderMock = CreateMockToProvide(exchangeRates);

            var fakeExchangeRateService = new FakeCosmosDbService<ExchangeRate>();

            var updateLogic = new ExchangeRateUpdateLogic(exchangeRateProviderMock.Object,
                                                          subscriptionServiceMock.Object,
                                                          fakeExchangeRateService);

            updateLogic.FetchAndUpdateExchangeRates(_loggerMock.Object);

            var idGenerator = new ExchangeRateIdGenerator();
            exchangeRates.ForEach(exchangeRate =>
                exchangeRate.Id = idGenerator.GenerateIdFor(exchangeRate));

            fakeExchangeRateService.VerifyWhetherUpsertedItemsAre(exchangeRates);
            subscriptionServiceMock.Verify();
            exchangeRateProviderMock.Verify();
        }

    }// end of class ExchangeRateUpdateLogicTest

}// end of namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
