using System;
using System.Text.Json.Serialization;

using CurrencyMonitor.DataModels;

using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class CosmosDbPartitionedItemTest
    {
        private void LoadItemType_Throws<ItemType>() where ItemType : class
        {
            Exception thrownException = Assert.ThrowsAny<Exception>(
                () => CosmosDbPartitionedItem<ItemType>.PartitionKeyPath);

            while (thrownException.InnerException != null)
                thrownException = thrownException.InnerException;

            Assert.Equal(typeof(NotSupportedException), thrownException.GetType());
        }

        [Fact]
        public void LoadItemType_WhenNoKey_ThenThrow()
        {
            LoadItemType_Throws<TestClassItemNoKey>();
        }

        [Fact]
        public void LoadItemType_WhenKeyLacksJson_ThenThrow()
        {
            LoadItemType_Throws<TestClassItemKeyLacksJson>();
        }

        [Fact]
        public void LoadItemType_WhenTwoKeys_ThenThrow()
        {
            LoadItemType_Throws<TestClassItemWithTwoKeys>();
        }

        [Fact]
        public void GetPartitionKeyPath()
        {
            Assert.Equal("/key", CosmosDbPartitionedItem<TestClassItem>.PartitionKeyPath);
        }

        [Fact]
        public void GetPartitionKeyValue()
        {
            var obj = new TestClassItem { Key = "Schlüssel", Value = "Wert" };
            var dbItem = new CosmosDbPartitionedItem<TestClassItem>(obj);
            Assert.Equal(obj.Key, dbItem.PartitionKeyValue);
            obj.Key = "Etwas Anderes";
            Assert.Equal(obj.Key, dbItem.PartitionKeyValue);
        }
    }

    /// <summary>
    /// Dieser Type sollte als Element der Datenbank gut funktionieren.
    /// </summary>
    internal class TestClassItem
    {
        [PartitionKey]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// Partitionsschlüssel nicht vorhanden.
    /// </summary>
    internal class TestClassItemNoKey
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// Es fehlt dem Partitionsschlüssel ein Name aus JSON-Serialisierung.
    /// </summary>
    internal class TestClassItemKeyLacksJson
    {
        [PartitionKey]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// Nur ein einzelner Partitionsschlüssel ist erlaubt.
    /// </summary>
    internal class TestClassItemWithTwoKeys
    {
        [PartitionKey]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [PartitionKey]
        public string Value { get; set; }
    }

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
