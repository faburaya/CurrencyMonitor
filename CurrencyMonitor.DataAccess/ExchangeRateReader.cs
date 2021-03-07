using System;
using System.Text.RegularExpressions;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Liest den Wert des Wechselkurses aus dem HTML-Text einer Webseite.
    /// </summary>
    /// <remarks>Diese implementierung setzt auf https://themoneyconverter.com/ </remarks>
    public class ExchangeRateReader : IExchangeRateReader
    {
        private static readonly Regex exchangeRateRegex =
            new Regex(@">1 ([A-Z]{3}) = (\d+[,.]\d+) ([A-Z]{3})<", RegexOptions.Compiled);

        public bool TryRead(string hypertext,
                            out DataModels.ExchangePair exchange,
                            out double rate)
        {
            exchange = null;
            rate = 0.0;

            var match = exchangeRateRegex.Match(hypertext);
            if (!match.Success)
            {
                return false;
            }

            string currencyCode1 = match.Groups[1].Value;
            string currencyCode2 = match.Groups[3].Value;
            exchange = new DataModels.ExchangePair(currencyCode1, currencyCode2);

            rate = double.Parse(match.Groups[2].Value);
            if (exchange.PrimaryCurrencyCode == currencyCode2)
            {
                rate = 1 / rate;
            }

            return true;
        }

    }// end of class ExchangeRateReader

}// end of namespace CurrencyMonitor.DataAccess
