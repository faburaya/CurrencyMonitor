using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Reusable.DataAccess;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
{
    using NotifyParams = Tuple<SubscriptionForExchangeRate, ExchangeRate>;

    public class NotificationLogicTest
    {
        private readonly Mock<ILogger> _loggerMock;

        public NotificationLogicTest()
        {
            _loggerMock = new Mock<ILogger>(MockBehavior.Loose);
        }

        private static Mock<ICosmosDbService<SubscriptionForExchangeRate>> CreateMockDbServiceReturning(
            IEnumerable<SubscriptionForExchangeRate> subscriptions)
        {
            var mock = new Mock<ICosmosDbService<SubscriptionForExchangeRate>>(MockBehavior.Strict);

            mock.Setup(obj =>
                obj.QueryAsync(It.IsAny<Func<IOrderedQueryable<SubscriptionForExchangeRate>, IQueryable<SubscriptionForExchangeRate>>>())
            ).ReturnsAsync(subscriptions);

            return mock;
        }
        
        private static Mock<ISubscriberNotifier> CreateMockToNotify(IEnumerable<NotifyParams> calls)
        {
            var mock = new Mock<ISubscriberNotifier>(MockBehavior.Strict);

            foreach (var expectedCall in calls)
            {
                mock.Setup(obj =>
                    obj.Notify(
                        It.Is<SubscriptionForExchangeRate>(param => param.Equals(expectedCall.Item1)),
                        It.Is<ExchangeRate>(param => param.Equals(expectedCall.Item2)))
                );
            }

            return mock;
        }

        [Fact]
        public void VerifyExchangeRateAgainstSubscriptionsAndNotify_WhenNoSubscriber_ThenDoNothing()
        {
            var cosmosDbServiceMock =
                CreateMockDbServiceReturning(new List<SubscriptionForExchangeRate> { });

            var notifierMock = CreateMockToNotify(new List<NotifyParams> { });

            var notificationLogic =
                new SubscriberNotificationLogic(cosmosDbServiceMock.Object, notifierMock.Object);

            var exchangeRate = ExchangeRate.CreateFrom(1.0, "EUR", 1.0, "BRL");

            notificationLogic
                .VerifyExchangeRateAgainstSubscriptionsAndNotify(exchangeRate, _loggerMock.Object)
                .Wait();

            cosmosDbServiceMock.Verify();
            notifierMock.Verify();
        }

        [Fact]
        public void VerifyExchangeRateAgainstSubscriptionsAndNotify_WhenOneSubscriber_IfRateDoesNotHitTarget_ThenDoNothing()
        {
            var subscription = new SubscriptionForExchangeRate
            {
                EMailAddress = "mensch@mail.de",
                CodeCurrencyToBuy = "BRL",
                CodeCurrencyToSell = "EUR",
                TargetPriceOfSellingCurrency = new decimal(7.0),
                LastNotification = DateTime.UnixEpoch
            };

            var cosmosDbServiceMock = CreateMockDbServiceReturning(
                new List<SubscriptionForExchangeRate> { subscription });

            var notifierMock = CreateMockToNotify(new List<NotifyParams> { });

            var notificationLogic =
                new SubscriberNotificationLogic(cosmosDbServiceMock.Object, notifierMock.Object);

            var exchangeRate = ExchangeRate.CreateFrom(1.0, "EUR", 1.0, "BRL");

            notificationLogic
                .VerifyExchangeRateAgainstSubscriptionsAndNotify(exchangeRate, _loggerMock.Object)
                .Wait();

            cosmosDbServiceMock.Verify();
            notifierMock.Verify();
        }

        [Fact]
        public void VerifyExchangeRateAgainstSubscriptionsAndNotify_WhenManySubscribers_IfRateHitsSomeTargets_ThenNotifyOnlyThoseSubscribers()
        {
            var exchangeRate = ExchangeRate.CreateFrom(1.0, "EUR", 8.0, "BRL");

            var subscriptions = new List<SubscriptionForExchangeRate> {
                new SubscriptionForExchangeRate
                {
                    Label = "Muss benachrichtigt werden",
                    EMailAddress = "null@mail.de",
                    CodeCurrencyToBuy = "BRL",
                    CodeCurrencyToSell = "EUR",
                    TargetPriceOfSellingCurrency = new decimal(7.0),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Label = "Muss auch benachrichtigt werden",
                    EMailAddress = "eins@mail.de",
                    CodeCurrencyToBuy = "EUR",
                    CodeCurrencyToSell = "BRL",
                    TargetPriceOfSellingCurrency = new decimal(0.12),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Label = "Sollte keine Benachrichtigung erhalten: Preis ist zu hoch",
                    EMailAddress = "zwei@mail.de",
                    CodeCurrencyToBuy = "BRL",
                    CodeCurrencyToSell = "EUR",
                    TargetPriceOfSellingCurrency = new decimal(9.0),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Label = "Sollte keine Benachrichtigung erhalten: verkehrter Wechsel",
                    EMailAddress = "drei@mail.de",
                    CodeCurrencyToBuy = "EUR",
                    CodeCurrencyToSell = "BRL",
                    TargetPriceOfSellingCurrency = new decimal(7.0),
                    LastNotification = DateTime.UnixEpoch
                },
                new SubscriptionForExchangeRate
                {
                    Label = "Sollte heute keine weitere Benachrichtigung erhalten",
                    EMailAddress = "vier@mail.de",
                    CodeCurrencyToBuy = "BRL",
                    CodeCurrencyToSell = "EUR",
                    TargetPriceOfSellingCurrency = new decimal(7.0),
                    LastNotification = DateTime.UtcNow.Date // today (UTC)
                },
            };

            var cosmosDbServiceMock = CreateMockDbServiceReturning(subscriptions);

            var notifierMock = CreateMockToNotify(new List<NotifyParams> {
                new NotifyParams(subscriptions[0], exchangeRate),
                new NotifyParams(subscriptions[1], exchangeRate),
            });

            var notificationLogic =
                new SubscriberNotificationLogic(cosmosDbServiceMock.Object, notifierMock.Object);

            notificationLogic
                .VerifyExchangeRateAgainstSubscriptionsAndNotify(exchangeRate, _loggerMock.Object)
                .Wait();

            cosmosDbServiceMock.Verify();
            notifierMock.Verify();
        }

    }// end of class NotificationLogicTest

}// end of namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
