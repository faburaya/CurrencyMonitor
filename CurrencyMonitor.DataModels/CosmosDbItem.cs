using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Minimal Definition für ein Objekt, das in Azure Cosmos Datenbank gespeichert werden soll.
    /// Jede solches Objekt muss sich davon ableiten.
    /// </summary>
    public class CosmosDbItem
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
