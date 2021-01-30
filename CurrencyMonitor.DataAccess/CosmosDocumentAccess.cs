using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Gewährt Zugang auf ein Dokument in Azure Cosmos Datenbank.
    /// </summary>
    public class CosmosDocumentAccess<DataType>
        : ITableAccess<DataType>
        where DataType : DataModels.CosmosDbItem
    {
        private CosmosDbService<DataType> DatabaseService { get; }

        private List<Task> Insertions { get; }

        public CosmosDocumentAccess(CosmosDbService<DataType> dbService)
        {
            this.DatabaseService = dbService;
            this.Insertions = new List<Task>();
        }

        public bool IsEmpty()
        {
            return (DatabaseService.GetItemCount().Result > 0);
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
