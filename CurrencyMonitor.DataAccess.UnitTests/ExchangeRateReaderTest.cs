using System;

using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class ExchangeRateReaderTest
    {
        [Fact]
        public void Constructor_WhenCodesValid()
        {
            new ExchangeRateReader("EUR", "BRL");
        }

        [Fact]
        public void Constructor_WhenCodesInvalid_ThenThrow()
        {
            Assert.Throws<ArgumentException>(() => new ExchangeRateReader("EUR", "Ungültiges"));
            Assert.Throws<ArgumentException>(() => new ExchangeRateReader("Ungültiges", "BRL"));
        }

        [Fact]
        public void TryRead_WhenNotCompliant_ThenGiveZeroAndFalse()
        {
            double rate;
            var reader = new ExchangeRateReader("EUR", "BRL");
            Assert.False(reader.TryRead("<div>Keine nützliche Daten</div>", out rate));
            Assert.Equal(0.0, rate);
        }

        [Fact]
        public void TryRead_WhenCompliant_ThenGiveRateAndTrue()
        {
            double rate;
            var reader = new ExchangeRateReader("EUR", "BRL");
            Assert.True(
                reader.TryRead(@"
                    <div class=""cc-rate"">
                        <label title=""Exchange Rate"" id=""elb"" accesskey=""x"" for=""cc-ratebox"">Umrechnungskurs (Kauf/Verkauf)</label>
                        <div tabindex=""4"" aria-labelledby=""elb"" id=""cc-ratebox"">BRL/EUR = 6,7805</div>
                    </div>", out rate));

            Assert.Equal(6.7805, rate);
        }
    }
}
