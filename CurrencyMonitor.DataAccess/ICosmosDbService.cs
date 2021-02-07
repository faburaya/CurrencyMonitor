using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Generische Schnittstelle, die Zugang auf die Azure Cosmos Datenbank gewährt.
    /// </summary>
    /// <typeparam name="ItemType">Der Typ in der Datenbank, mit dem man umgehen will.</typeparam>
    public interface ICosmosDbService<ItemType>
    {
        Task<IEnumerable<ItemType>> QueryAsync(Func<IOrderedQueryable<ItemType>, IQueryable<ItemType>>  query);

        Task<ItemType> GetItemAsync(string partitionKey, int id);

        Task<int> GetItemCountAsync();

        Task AddItemAsync(ItemType item);

        Task DeleteItemAsync(string partitionKey, int id);

        Task UpdateItemAsync(ItemType item);
    }
}