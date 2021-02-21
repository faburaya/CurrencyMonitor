﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Gewährt Zugang auf ein Element in Azure Cosmos Datenbank.
    /// </summary>
    public class CosmosDbItemAccess<DataType>
        : ITableAccess<DataType>
        where DataType : DataModels.CosmosDbItem
    {
        private ICosmosDbService<DataType> DatabaseService { get; }

        private List<Task> Insertions { get; }

        public CosmosDbItemAccess(ICosmosDbService<DataType> dbService)
        {
            this.DatabaseService = dbService;
            this.Insertions = new List<Task>();
        }

        public bool IsEmpty()
        {
            return (DatabaseService.GetItemCountAsync().Result > 0);
        }

        public void Insert(DataType obj)
        {
            Insertions.Add(DatabaseService.AddItemAsync(obj));
        }

        public void Commit()
        {
            Task.WaitAll(Insertions.ToArray());
            Insertions.Clear();
        }
    }
}