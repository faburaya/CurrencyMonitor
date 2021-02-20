using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Minimal Definition für ein Objekt, das in Azure Cosmos Datenbank gespeichert werden soll.
    /// Jede solches Objekt muss sich davon ableiten.
    /// </summary>
    public abstract class CosmosDbItem
    {
        [Required]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public abstract string PartitionKeyValue { get; }
    }
}
