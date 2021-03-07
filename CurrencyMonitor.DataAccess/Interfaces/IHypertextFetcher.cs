using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    public interface IHypertextFetcher
    {
        Task<string> DownloadFrom(string url);
    }
}