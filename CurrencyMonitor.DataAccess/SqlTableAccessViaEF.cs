using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Gewährt Zugang (durch EF) auf eine Tabelle in einer SQL-Datenbank.
    /// </summary>
    /// <remarks>Threadsicherheit is NICHT gewährleistet.</remarks>
    /// <typeparam name="DataType">Der enthaltende Typ von Daten.</typeparam>
    public class SqlTableAccessViaEF<DataType> : ITableAccess<DataType> where DataType : class
    {
        private CurrencyMonitorContext DatabaseContext { get; }

        private DbSet<DataType> Rows { get; }

        public SqlTableAccessViaEF(CurrencyMonitorContext dbContext)
        {
            _rowCountInTransaction = 0;

            this.DatabaseContext = dbContext;

            Type dataType = typeof(DataType);

            if (dataType == typeof(DataModels.RecognizedCurrency))
            {
                this.Rows = dbContext.RecognizedCurrency as DbSet<DataType>;
            }
            else if (dataType == typeof(DataModels.SubscriptionForExchangeRate))
            {
                this.Rows = dbContext.SubscriptionForExchangeRate as DbSet<DataType>;
            }
            else
            {
                throw new NotSupportedException(
                    $"Diese Implementierung unterstützt den Typ {dataType.Name} nicht!");
            }
        }

        public bool IsEmpty() => !Rows.Any();

        private int _rowCountInTransaction;

        public void Insert(DataType obj)
        {
            Rows.Add(obj);
            ++_rowCountInTransaction;
        }

        public void Commit()
        {
            if (_rowCountInTransaction > 0)
            {
                DatabaseContext.SaveChanges();
                _rowCountInTransaction = 0;
            }
        }
    }
}
