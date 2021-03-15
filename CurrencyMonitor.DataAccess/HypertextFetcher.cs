using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Gewährt die Fähigkeit zum Abrufen des Hypertext aus einer Webseite.
    /// </summary>
    public class HypertextFetcher : IHypertextFetcher
    {
        private static readonly HttpClient httpClient;

        private static readonly TimeSpan timeSlotForRetry;

        static HypertextFetcher()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            timeSlotForRetry = new TimeSpan((long)(100 /*ms*/ * 1e4  /*ticks per ms*/));
        }

        private readonly uint _maxRetries;

        public HypertextFetcher(uint maxRetries = 5)
        {
            _maxRetries = maxRetries;
        }

        public async Task<string> DownloadFrom(string url)
        {
            uint attempt = 1;
            while (true)
            {
                try
                {
                    return await httpClient.GetStringAsync(url);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains(/*HTTP Fehler*/" 403 "))
                {
                    if (attempt <= _maxRetries)
                    {
                        System.Threading.Thread.Sleep(
                            RetryStrategy.CalculateExponentialBackoff(timeSlotForRetry, attempt)
                        );
                        ++attempt;
                        continue;
                    }

                    throw new ApplicationException($"Das Abrufen von Hypertext aus der URL {url} is gescheitert!", ex);
                }
            }
        }

    }// end of class HypertextFetcher

}// end of namespace CurrencyMonitor.DataAccess
