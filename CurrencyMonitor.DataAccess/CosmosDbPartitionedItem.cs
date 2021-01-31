using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

using CurrencyMonitor.DataModels;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Einrichtung um einen Typ herum, der als Element der Datenbank dient.
    /// </summary>
    public class CosmosDbPartitionedItem<ItemType> where ItemType : class
    {
        private ItemType Item { get; }

        public CosmosDbPartitionedItem(ItemType item)
        {
            this.Item = item;
        }

        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this.Item);
        }

        private static readonly PropertyInfo partitionKeyProperty;

        public string PartitionKeyValue => (string)partitionKeyProperty.GetValue(this.Item);

        public static string PartitionKeyPath { get; }

        public static string ContainerName { get; }

        /// <summary>
        /// Initialisiert die Einrichtung.
        /// </summary>
        /// <summary>
        /// Findet den Pfad des Partitionsschlüssels, der von einem Attribut markiert ist,
        /// und dessen Namen den Namen in der JSON-Serialisierung gleicht.
        /// </summary>
        static CosmosDbPartitionedItem()
        {
            var containerAttribute = typeof(ItemType).GetCustomAttribute<CosmosContainerAttribute>();
            if (containerAttribute == null)
            {
                throw new NotSupportedException($"Der Name des Containers für den Datenbank: an dem Typ {typeof(ItemType).Name} fehlt das Attribut [CosmosContainer(Name)]!");
            }

            ContainerName = containerAttribute.Name;

            IEnumerable<PropertyInfo> properties = (
                from property in typeof(ItemType).GetProperties()
                where property.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null
                select property
            );

            if (!properties.Any())
            {
                throw new NotSupportedException($"Der Pfad für den Partitionsschlüssel der Datenbank kann nicht festgestellt werden: an dem Typ {typeof(ItemType).Name} fehlt ein Property mit dem Attribut [CosmosPartitionKey]!");
            }

            if (properties.Count() > 1)
            {
                throw new NotSupportedException($"Der Pfad für den Partitionsschlüssel der Datenbank kann nicht festgestellt werden: der Typ {typeof(ItemType).Name} hat mehr als ein Property mit dem Attribut [CosmosPartitionKey]!");
            }

            partitionKeyProperty = properties.First();

            var jsonSerialization = partitionKeyProperty.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonSerialization != null)
            {
                PartitionKeyPath = $"/{jsonSerialization.Name}";
                return;
            }

            throw new NotSupportedException($"Der Pfad für den Partitionsschlüssel der Datenbank kann nicht festgestellt werden: der Partitionsschlüssel {partitionKeyProperty.Name} fehlt das Attribut [JsonPropertyName].");
        }

    }// end of class CosmosDbItem

}// end of namespace CurrencyMonitor.DataAccess
