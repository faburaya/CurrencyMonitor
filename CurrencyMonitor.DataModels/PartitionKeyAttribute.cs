using System;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Attribute für ein Property, das als Partitions Schlüssel in der Datenbank dient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PartitionKeyAttribute : Attribute
    {
        public bool IsDatabasePartitionKey => true;

    }
}
