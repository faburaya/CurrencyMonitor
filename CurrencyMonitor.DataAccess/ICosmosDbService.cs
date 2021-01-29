﻿using System.Collections.Generic;
using System.Threading.Tasks;

using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Generische Schnittstelle, die Zugang auf die Azure Cosmos Datenbank gewährt.
    /// </summary>
    /// <typeparam name="ItemType">Der Typ in der Datenbank, mit dem man umgehen will.</typeparam>
    public interface ICosmosDbService<ItemType>
    {
        Task AddItemAsync(ItemType item);
        Task DeleteItemAsync(string key);
        Task<ItemType> GetItemAsync(string key);
        Task<IEnumerable<ItemType>> QueryAsync(string queryString);
        Task UpdateItemAsync(string key, ItemType item);
    }
}