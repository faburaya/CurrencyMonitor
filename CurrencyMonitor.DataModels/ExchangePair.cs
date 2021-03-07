using System;

namespace CurrencyMonitor.DataModels
{
    /// <summary>
    /// Stellt ein Paar von Währungen in einem Wechselkurs dar.
    /// </summary>
    /// <remarks>
    /// Diese Klasse ermöglicht, dass die gegebenen Paare beispielweise EUR:BRL und BRL:EUR gleich sind.
    /// Daher kann man den Wechselkurs nur einmal für 'EUR/BRL = x' aus dem Internet abrufen, und für
    /// BRL/EUR nimmt man 1/x. Das soll gleichzeitig rechnerische und speicherliche Ressourcen sparen.
    /// </remarks>
    public class ExchangePair : IComparable<ExchangePair>, IEquatable<ExchangePair>
    {
        public string PrimaryCurrencyCode { get; }

        public string SecondaryCurrencyCode { get; }

        public ExchangePair(string currencyCode1, string currencyCode2)
        {
            var currencyPair = DetermineOrderOfPair(currencyCode1, currencyCode2);
            this.PrimaryCurrencyCode = currencyPair.Item1;
            this.SecondaryCurrencyCode = currencyPair.Item2;
        }

        private static (string, string) DetermineOrderOfPair(string currencyCode1, string currencyCode2)
        {
            if (currencyCode1.CompareTo(currencyCode2) <= 0)
            {
                return (currencyCode1, currencyCode2);
            }
            else
            {
                return (currencyCode2, currencyCode1);
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 7;
            hashCode = 31 * hashCode + PrimaryCurrencyCode.GetHashCode();
            hashCode = 31 * hashCode + SecondaryCurrencyCode.GetHashCode();
            return hashCode;
        }

        public int CompareTo(ExchangePair other)
        {
            int comparison = this.PrimaryCurrencyCode.CompareTo(other.PrimaryCurrencyCode);
            if (comparison != 0)
            {
                return comparison;
            }

            return this.SecondaryCurrencyCode.CompareTo(other.SecondaryCurrencyCode);
        }

        public bool Equals(ExchangePair other)
        {
            return this.PrimaryCurrencyCode.Equals(other.PrimaryCurrencyCode)
                && this.SecondaryCurrencyCode.Equals(other.SecondaryCurrencyCode);
        }

    }// end of class ExchangePair

}// end of namespace CurrencyMonitor.DataModels
