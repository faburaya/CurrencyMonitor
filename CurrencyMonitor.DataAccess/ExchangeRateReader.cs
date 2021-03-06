using System;
using System.Text.RegularExpressions;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Liest den Wert des Wechselkurses aus dem HTML-Text einer Webseite.
    /// </summary>
    /// <remarks>Diese implementierung setzt auf https://themoneyconverter.com/ </remarks>
    public class ExchangeRateReader
    {
        private readonly static Regex currencyCodeRegex;

        static ExchangeRateReader()
        {
            currencyCodeRegex = new Regex("^[A-Z]{3}$", RegexOptions.Compiled);
        }

        private void ValidateCurrencyCode(string currencyCode)
        {
            if (!currencyCodeRegex.IsMatch(currencyCode))
            {
                throw new ArgumentException($"{currencyCode} is kein gültiges Code für eine Währung!");
            }
        }

        private readonly Regex _exchangeRateRegex;

        public ExchangeRateReader(string sellingCurrencyCode, string buyingCurrencyCode)
        {
            ValidateCurrencyCode(sellingCurrencyCode);
            ValidateCurrencyCode(buyingCurrencyCode);

            _exchangeRateRegex =
                new Regex($@"{buyingCurrencyCode}/{sellingCurrencyCode} = (\d+,\d+)", RegexOptions.Compiled);
        }

        /// <summary>
        /// Versucht, dem Hypertext den Wechselkurs zu entnehmen.
        /// </summary>
        /// <param name="hyperText">Der zu lesende HTML-Inhalt.</param>
        /// <param name="exchangeRate">Wird den Wechselkurs zurückgewiesen.</param>
        /// <returns>Ob das Lesen von dem Wert des Wechselkurses gelungen ist.</returns>
        public bool TryRead(string hyperText, out double exchangeRate)
        {
            exchangeRate = 0.0;

            var match = _exchangeRateRegex.Match(hyperText);
            if (match.Success)
            {
                return double.TryParse(match.Groups[1].Value, out exchangeRate);
            }

            return false;
        }

    }// end of class ExchangeRateReader

}// end of namespace CurrencyMonitor.DataAccess
