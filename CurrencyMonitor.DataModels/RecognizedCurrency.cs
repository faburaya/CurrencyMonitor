using System.ComponentModel.DataAnnotations;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Stellt eine anerkannte Währung, etwa Real oder Euro.
    /// </summary>
    public class RecognizedCurrency
    {
        public int ID { get; set; }

        /// <summary>
        /// Code nach ISO-4217.
        /// </summary>
        [Display(Name = "ISO-Code")]
        public string Code { get; set; }

        [Display(Name = "Währung")]
        public string Name { get; set; }

        [Display(Name = "Symbol")]
        public string Symbol { get; set; }

        [Display(Name = "Gültigkeit (Länder)")]
        public string Country { get; set; }
    }
}
