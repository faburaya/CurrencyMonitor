
namespace CurrencyMonitor.DataModels
{
    public interface ICosmosDbItem
    {
        string PartitionKey { get; }
    }
}
