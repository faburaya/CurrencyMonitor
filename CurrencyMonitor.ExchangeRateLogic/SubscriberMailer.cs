using System.Net.Mail;

using CurrencyMonitor.DataModels;
using Reusable.Utils;

namespace CurrencyMonitor.ExchangeRateLogic
{
    /// <summary>
    /// Benachrichtigt den Abonnenten per E-Mail.
    /// </summary>
    public class SubscriberMailer : ISubscriberNotifier
    {
        private readonly SmtpClient _smtpClient;

        private readonly string _senderEmail;

        /// <summary>
        /// Erstellt ein neues Objekt.
        /// </summary>
        /// <param name="host">URL des SMTP-Servers.</param>
        /// <param name="port">TCP-Port des SMTP-Servers.</param>
        /// <param name="senderEmail">E-Mail-Adresse des Senders.</param>
        public SubscriberMailer(string host,
                                int port,
                                string senderEmail,
                                PasswordBasedCredential credential)
        {
            _senderEmail = senderEmail;
            _smtpClient = new SmtpClient(host, port)
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(credential.UserId, credential.Password)
            };
        }

        public void Notify(SubscriptionForExchangeRate subscription, ExchangeRate exchangeRate)
        {
            if (exchangeRate.PriceOfPrimaryCurrency < 1.0)
            {
                exchangeRate = exchangeRate.Revert();
            }

            var message = new MailMessage(
                _senderEmail,
                subscription.EMailAddress,
                $"Wechselkurs {subscription.CodeCurrencyToSell}->{subscription.CodeCurrencyToBuy} hat den gewünschten Wert erreicht",
                $"1 {exchangeRate.PrimaryCurrencyCode} = {exchangeRate.PriceOfPrimaryCurrency} {exchangeRate.SecondaryCurrencyCode} [{exchangeRate.Timestamp}]\n");

            _smtpClient.Send(message);
        }

    }// end of class SubscriberMailer

}// end of namespace CurrencyMonitor.ExchangeRateLogic
