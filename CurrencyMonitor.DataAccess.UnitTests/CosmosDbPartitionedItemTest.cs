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
        public void LoadItemType_WhenNoContainer_ThenThrow()
        {
            LoadItemType_Throws<TestClassItemNoContainer>();
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
    [CosmosContainer(Name = "box")]
    internal class TestClassItem
    {
        [CosmosPartitionKey]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// ihm felht der Name des Containers.
    /// </summary>
    internal class TestClassItemNoContainer
    {
        [CosmosPartitionKey]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// Partitionsschlüssel nicht vorhanden.
    /// </summary>
    [CosmosContainer(Name = "box")]
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
    [CosmosContainer(Name = "box")]
    internal class TestClassItemKeyLacksJson
    {
        [CosmosPartitionKey]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Dieser Type funktioniert nicht als Element:
    /// Nur ein einzelner Partitionsschlüssel ist erlaubt.
    /// </summary>
    [CosmosContainer(Name = "box")]
    internal class TestClassItemWithTwoKeys
    {
        [CosmosPartitionKey]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [CosmosPartitionKey]
        public string Value { get; set; }
    }

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
