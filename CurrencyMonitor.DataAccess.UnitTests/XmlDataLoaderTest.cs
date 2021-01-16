using System;
using System.IO;

using Moq;
using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class XmlDataLoaderTest
    {
        private string TestFilePath => "test_XmlDataLoader.xml";

        private string SchemaDeploymentFilePath => "deployment.xsd";

        private string DeploymentXmlNamespace => "http://www.currencymonitor.com/deployment";

        private string CreateValidXml(string[] innerXmlElements)
        {
            var buffer = new System.Text.StringBuilder();
            foreach (string entry in innerXmlElements)
                buffer.AppendLine(entry);

            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8"" ?><deployment xmlns=""http://www.currencymonitor.com/deployment""><currencies>{0}</currencies></deployment>", buffer.ToString());
        }

        [Theory]
        [InlineData("<ill></formed>", typeof(System.Xml.XmlException))]
        [InlineData("<root></root>", typeof(System.Xml.Schema.XmlSchemaException))]
        public void Load_Currencies_WhenInvalidXml_ThenThrow(string xmlContent, Type exceptionType)
        {
            File.WriteAllText(TestFilePath, xmlContent);

            var dataLoader = new XmlDataLoader(
                new XmlMetadata("http://nichts.de", TestFilePath, SchemaDeploymentFilePath));

            var mockDbAccess = new Mock<ITableAccess<DataModels.RecognizedCurrency>>(MockBehavior.Strict);
            mockDbAccess.Setup(obj => obj.IsEmpty()).Returns(true);
            Assert.Throws(exceptionType, () => dataLoader.Load(mockDbAccess.Object));
            mockDbAccess.Verify();

            File.Delete(TestFilePath);
        }

        [Fact]
        public void Load_Currencies_WhenXmlEmpty_ThenLoadNothing()
        {
            File.WriteAllText(TestFilePath, CreateValidXml(new string[] { "" }));

            var dataLoader = new XmlDataLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            var mockDbAccess = new Mock<ITableAccess<DataModels.RecognizedCurrency>>(MockBehavior.Strict);
            mockDbAccess.Setup(obj => obj.IsEmpty()).Returns(true);
            mockDbAccess.Setup(obj => obj.Commit());
            dataLoader.Load(mockDbAccess.Object);
            mockDbAccess.Verify();

            File.Delete(TestFilePath);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Load_Currencies_WhenOneAvailable_ThenLoadIfTableNotSeeded(bool isTableSeeded)
        {
            File.WriteAllText(TestFilePath,
                CreateValidXml(new string[] {
                    @"<entry code=""EUR"" name=""Euro"" symbol=""€"" country=""EU"" />" 
                })
            );

            var dataLoader = new XmlDataLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            var mockDbAccess = new Mock<ITableAccess<DataModels.RecognizedCurrency>>(MockBehavior.Strict);
            mockDbAccess.Setup(obj => obj.IsEmpty()).Returns(!isTableSeeded);
            mockDbAccess.Setup(obj => obj.Insert(It.IsAny<DataModels.RecognizedCurrency>()));
            mockDbAccess.Setup(obj => obj.Commit());

            dataLoader.Load(mockDbAccess.Object);

            if (!isTableSeeded)
            {
                mockDbAccess.Verify(
                    dac => dac.Insert(
                        It.Is<DataModels.RecognizedCurrency>(obj =>
                            obj.Code == "EUR" && obj.Name == "Euro" && obj.Symbol == "€" && obj.Country == "EU"))
                , Times.Once);

                mockDbAccess.Verify(dac => dac.Commit(), Times.Once);
            }

            File.Delete(TestFilePath);
        }

        [Fact]
        public void Load_Currencies_WhenManyAvailable_ThenLoadAll()
        {
            File.WriteAllText(TestFilePath,
                CreateValidXml(new string[] {
                    @"<entry code=""EUR"" name=""Euro"" symbol=""€"" country=""EU"" />",
                    @"<entry code=""USD"" name=""Dollar"" symbol=""$"" country=""USA"" />",
                    @"<entry code=""BRL"" name=""Real"" symbol=""R$"" country=""Brasilien"" />"
                })
            );

            var dataLoader = new XmlDataLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            var mockDbAccess = new Mock<ITableAccess<DataModels.RecognizedCurrency>>(MockBehavior.Strict);
            mockDbAccess.Setup(obj => obj.IsEmpty()).Returns(true);
            mockDbAccess.Setup(obj => obj.Insert(It.IsAny<DataModels.RecognizedCurrency>()));
            mockDbAccess.Setup(obj => obj.Commit());

            dataLoader.Load(mockDbAccess.Object);

            mockDbAccess.Verify(
                dac => dac.Insert(
                    It.Is<DataModels.RecognizedCurrency>(obj =>
                        obj.Code == "EUR" && obj.Name == "Euro" && obj.Symbol == "€" && obj.Country == "EU"))
            , Times.Once);

            mockDbAccess.Verify(
                dac => dac.Insert(
                    It.Is<DataModels.RecognizedCurrency>(obj =>
                        obj.Code == "USD" && obj.Name == "Dollar" && obj.Symbol == "$" && obj.Country == "USA"))
            , Times.Once);

            mockDbAccess.Verify(
                dac => dac.Insert(
                    It.Is<DataModels.RecognizedCurrency>(obj =>
                        obj.Code == "BRL" && obj.Name == "Real" && obj.Symbol == "R$" && obj.Country == "Brasilien"))
            , Times.Once);

            mockDbAccess.Verify(dac => dac.Commit(), Times.Once);

            File.Delete(TestFilePath);
        }

    }// end of class XmlDataLoaderTest

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
