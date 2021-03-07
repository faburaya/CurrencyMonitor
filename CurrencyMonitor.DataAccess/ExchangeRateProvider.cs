using System;
using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Bietet den Wechselkurs aus dem Internet an.
    /// </summary>
    public class ExchangeRateProvider
    {
        private IHypertextFetcher _hypertextFetcher;

        private IExchangeRateReader _exchangeRateReader;

        /// <summary>
        /// Erstellt ein Objekt, sodass die Abhängigkeiten (zum Testen) injiziert werden können.
        /// </summary>
        /// <param name="hypertextFetcher">Ein Objekt, das den Hypertext einer Webseite abrufen kann.</param>
        /// <param name="exchangeRateReader">Ein Objekt, das den Wechselkurs aus Hypertext ablesen kann.</param>
        public ExchangeRateProvider(IHypertextFetcher hypertextFetcher,
                                    IExchangeRateReader exchangeRateReader)
        {
            this._hypertextFetcher = hypertextFetcher;
            this._exchangeRateReader = exchangeRateReader;
        }

        public ExchangeRateProvider()
            : this(new HypertextFetcher(), new ExchangeRateReader())
        {
        }

        /// <summary>
        /// Gibt den neuesten Wert des gegebenen Wechselkurs.
        /// </summary>
        /// <param name="exchange">Das Paar von Währungen, die den Wechselkurs bilden.</param>
        /// <returns></returns>
        /// <remarks>Diese implementierung setzt auf https://themoneyconverter.com/ </remarks>
        public async Task<DataModels.ExchangeRate> GetLatestRateFor(DataModels.ExchangePair exchange)
        {
            string url = $"https://themoneyconverter.com/DE/{exchange.PrimaryCurrencyCode}/{exchange.SecondaryCurrencyCode}";
            string hypertext = await _hypertextFetcher.DownloadFrom(url);
            if (!_exchangeRateReader.TryRead(hypertext,
                                             out DataModels.ExchangePair readExchange,
                                             out double rate))
            {
                throw new ApplicationException($"Es ist nicht gelungen, den Wechselkurs aus {url} abzulesen!");
            }

            return new DataModels.ExchangeRate(readExchange, rate);
        }

    }// end of class ExchangeRateProvider

}// end of namespace CurrencyMonitor.DataAccess
