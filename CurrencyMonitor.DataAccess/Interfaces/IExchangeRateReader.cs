namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Entnimmt aus dem gegebenen Hypertext den Wert eines gezielten Wechselkurses.
    /// </summary>
    public interface IExchangeRateReader
    {
        /// <summary>
        /// Versucht, dem Hypertext den Wechselkurs zu entnehmen.
        /// </summary>
        /// <param name="hypertext">Der zu lesende HTML-Inhalt.</param>
        /// <param name="exchange">Wird die verkaufende und die kaufende Währung zugewiesen.</param>
        /// <param name="rate">Wird den Preis von der primären Währung zugewiesen.</param>
        /// <returns>Ob das Lesen von Wechselkurs gelungen ist.</returns>
        bool TryRead(string hypertext,
                     out DataModels.ExchangePair exchange,
                     out double rate);
    }
}