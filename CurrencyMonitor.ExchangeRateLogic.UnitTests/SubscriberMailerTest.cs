using System;
using System.Linq;

using Xunit;
using netDumbster.smtp;
using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
{
    public class SubscriberMailerTest : IDisposable
    {
        private const int _smtpServerPort = 25;
        private readonly SimpleSmtpServer _fakeSmtpServer;

        public SubscriberMailerTest()
        {
            _fakeSmtpServer = SimpleSmtpServer.Start(_smtpServerPort);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _fakeSmtpServer.Stop();
            _disposed = true;
        }

        ~SubscriberMailerTest()
        {
            Dispose(false);
        }

        private const string _senderEmailAddress = "sender@test.de";

        private ISubscriberNotifier CreateMailer()
        {
            var credentials = new Reusable.Utils.PasswordBasedCredential
            {
                UserId = "Benutzer",
                Password = "Kennwort"
            };
            return new SubscriberMailer(
                "localhost", _smtpServerPort, _senderEmailAddress, credentials);
        }

        [Fact]
        public void Instantiate()
        {
            CreateMailer();
        }

        [Fact]
        public void Notify()
        {
            ISubscriberNotifier mailer = CreateMailer();

            var subscription = new SubscriptionForExchangeRate {
                Id = "CAFEBABE",
                Label = "Abonnement",
                EMailAddress = "receiver@test.de",
                CodeCurrencyToBuy = "BRL",
                CodeCurrencyToSell = "EUR",
                TargetPriceOfSellingCurrency = new decimal(1.0),
                LastNotification = DateTime.UnixEpoch
            };

            ExchangeRate rate = ExchangeRate.CreateFrom(
                new ExchangePair(subscription.CodeCurrencyToBuy, subscription.CodeCurrencyToSell), 1.0);

            mailer.Notify(subscription, rate);

            Assert.Equal(1, _fakeSmtpServer.ReceivedEmailCount);
            SmtpMessage message = _fakeSmtpServer.ReceivedEmail.First();
            Assert.Equal(subscription.EMailAddress, message.ToAddresses.FirstOrDefault()?.ToString());
        }

    }// end of class SubscriberMailerTest

}// end of namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
