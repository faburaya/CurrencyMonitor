using System;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Gewährt eine Strategie für Wiederholungen von gescheiterten Operationen.
    /// </summary>
    internal class RetryStrategy
    {
        private static readonly Random randomGenerator = new Random(DateTime.Now.Millisecond);

        public static TimeSpan CalculateExponentialBackoff(TimeSpan timeSlot, uint attempt)
        {
            return randomGenerator.Next(1 << (int)attempt) * timeSlot;
        }
    }
}
