using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Reusable.DataAccess;
using Reusable.DataModels;

namespace CurrencyMonitor.ExchangeRateLogic.UnitTests
{
    public class FakeCosmosDbService<ItemType> : ICosmosDbService<ItemType>
        where ItemType : CosmosDbItem<ItemType>, IEquatable<ItemType>, IComparable<ItemType>
    {
        private SortedSet<ItemType> _upsertedItems = new SortedSet<ItemType>();

        public void Dispose()
        {
            _upsertedItems.Clear();
        }

        public Task<ItemType> AddItemAsync(ItemType item)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBatchAsync(string partitionKey, IList<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync(string partitionKey, string id)
        {
            throw new NotImplementedException();
        }

        public Task<ItemType> GetItemAsync(string partitionKey, string id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetItemCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ItemType>> QueryAsync(
            Func<IOrderedQueryable<ItemType>, IQueryable<ItemType>> query)
        {
            throw new NotImplementedException();
        }

        public async Task UpsertBatchAsync(IEnumerable<ItemType> items)
        {
            await Task.Run(() => {
                foreach (ItemType item in items)
                {
                    _upsertedItems.Add(item);
                }
            });
        }

        public Task UpsertItemAsync(string partitionKey, ItemType item)
        {
            throw new NotImplementedException();
        }

        public void VerifyWhetherUpsertedItemsAre(IEnumerable<ItemType> items)
        {
            List<ItemType> sortedItems = items.ToList();
            sortedItems.Sort();
            Assert.Equal(items, _upsertedItems);
        }

    }// end of class FakeCosmosDbService

}// end of namespace CurrencyMonitor.ExchangeRateLogic.UnitTests