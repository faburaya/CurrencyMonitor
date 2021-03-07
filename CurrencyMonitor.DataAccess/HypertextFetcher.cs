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
        private static HttpClient httpClient = new HttpClient();

        public async Task<string> DownloadFrom(string url)
        {
            try
            {
                return await httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Das Abrufen von Hypertext aus der URL {url} is gescheitert!", ex);
            }
        }

    }// end of class HypertextFetcher

}// end of namespace CurrencyMonitor.DataAccess
