using System.Threading.Tasks;

using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.DataAccess
{
    public interface IExchangeRateProvider
    {
        Task<ExchangeRate> GetLatestRateAsync(ExchangePair exchange);
    }
}