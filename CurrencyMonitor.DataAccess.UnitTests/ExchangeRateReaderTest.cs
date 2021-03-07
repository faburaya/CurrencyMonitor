using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class ExchangeRateReaderTest
    {
        [Fact]
        public void TryRead_WhenNotCompliant_ThenReturnFalse()
        {
            var reader = new ExchangeRateReader();
            Assert.False(reader.TryRead("<div>Keine nützliche Daten</div>",
                                        out DataModels.ExchangePair exchange,
                                        out double rate));
            Assert.Null(exchange);
            Assert.Equal(0.0, rate);
        }

        private void CheckFloatingPointValue(double expected, double actual)
        {
            double tolerance = 1e-5;
            Assert.InRange(actual, expected - tolerance, expected + tolerance);
        }

        [Fact]
        public void TryRead_WhenCompliant_IfDirectOrder_ThenReturnTrue()
        {
            var reader = new ExchangeRateReader();
            Assert.True(reader.TryRead(@"
                <div class=""cc-result"">
	                <output id=""res1"" for=""ta"">1 BRL = 0,14748 EUR</output>
                </div>",
                out DataModels.ExchangePair exchange,
                out double rate));

            Assert.NotNull(exchange);
            Assert.Equal("BRL", exchange.PrimaryCurrencyCode);
            Assert.Equal("EUR", exchange.SecondaryCurrencyCode);

            CheckFloatingPointValue(0.14748, rate);
        }

        [Fact]
        public void TryRead_WhenCompliant_IfReverseOrder_ThenReturnTrue()
        {
            var reader = new ExchangeRateReader();
            Assert.True(reader.TryRead(@"
                <div class=""cc-result"">
	                <output id=""res1"" for=""ta"">1 EUR = 6,7805 BRL</output>
                </div>",
                out DataModels.ExchangePair exchange,
                out double rate));

            Assert.NotNull(exchange);
            Assert.Equal("BRL", exchange.PrimaryCurrencyCode);
            Assert.Equal("EUR", exchange.SecondaryCurrencyCode);

            CheckFloatingPointValue(0.14748, rate);
        }

    }// end of class ExchangeRateReaderTest

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
