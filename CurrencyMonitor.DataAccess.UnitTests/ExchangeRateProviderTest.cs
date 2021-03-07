using System;
using System.Threading.Tasks;

using Moq;
using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class ExchangeRateProviderTest
    {
        private Mock<IHypertextFetcher> CreateMockForHypertextFetcher()
        {
            var hypertextFetcherTask = new Task<string>(() => "<html>wird sowieso nicht gelesen</html>");
            hypertextFetcherTask.Start();

            var mockHypertextFetcher = new Mock<IHypertextFetcher>(MockBehavior.Strict);
            mockHypertextFetcher.Setup(
                obj => obj.DownloadFrom(It.Is<string>(value => value.StartsWith("http")))
            ).Returns(hypertextFetcherTask);

            return mockHypertextFetcher;
        }

        [Fact]
        public void GetLatestRateFor_WhenCannotRead_ThenThrow()
        {
            var mockExchangeRateReader = new Mock<IExchangeRateReader>(MockBehavior.Strict);
            mockExchangeRateReader.Setup(
                obj => obj.TryRead(It.IsAny<string>(),
                                   out It.Ref<DataModels.ExchangePair>.IsAny,
                                   out It.Ref<double>.IsAny)
            ).Returns(false);

            Mock<IHypertextFetcher> mockHypertextFetcher = CreateMockForHypertextFetcher();
            var provider = new ExchangeRateProvider(mockHypertextFetcher.Object, mockExchangeRateReader.Object);

            var exception = Assert.Throws<AggregateException>(
                () => provider.GetLatestRateFor(new DataModels.ExchangePair("EUR", "BRL")).Result
            );
            Assert.IsType<ApplicationException>(exception.InnerException);

            mockHypertextFetcher.Verify();
            mockExchangeRateReader.Verify();
        }

        private delegate void CallbackTryRead(string ignored,
                                              out DataModels.ExchangePair exchange,
                                              out double rate);

        [Fact]
        public void GetLatestRateFor_WhenCanRead_ThenGiveExchangeRate()
        {
            const double expectedRate = 0.142857;
            DataModels.ExchangePair expectedExchange = new DataModels.ExchangePair("EUR", "BRL");
            CallbackTryRead callbackTryRead =
                delegate (string ignored, out DataModels.ExchangePair exchange, out double rate) {
                    exchange = expectedExchange;
                    rate = expectedRate;
                };

            var mockExchangeRateReader = new Mock<IExchangeRateReader>(MockBehavior.Strict);
            mockExchangeRateReader
                .Setup(
                    obj => obj.TryRead(It.IsAny<string>(),
                                       out It.Ref<DataModels.ExchangePair>.IsAny,
                                       out It.Ref<double>.IsAny)
                )
                .Returns(true)
                .Callback(callbackTryRead);

            Mock<IHypertextFetcher> mockHypertextFetcher = CreateMockForHypertextFetcher();
            var provider = new ExchangeRateProvider(mockHypertextFetcher.Object, mockExchangeRateReader.Object);
            DataModels.ExchangeRate actualExchangeRate = provider.GetLatestRateFor(expectedExchange).Result;

            Assert.Equal(expectedExchange.PrimaryCurrencyCode, actualExchangeRate.PrimaryCurrencyCode);
            Assert.Equal(expectedExchange.SecondaryCurrencyCode, actualExchangeRate.SecondaryCurrencyCode);

            const double tolerance = 1e-6;
            Assert.InRange(actualExchangeRate.PriceOfPrimaryCurrency,
                           expectedRate - tolerance,
                           expectedRate + tolerance);

            mockHypertextFetcher.Verify();
            mockExchangeRateReader.Verify();
        }

    }// end of class ExchangeRateProviderTest

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
