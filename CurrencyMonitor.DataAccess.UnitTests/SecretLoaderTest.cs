using System;
using System.IO;

using Xunit;

namespace CurrencyMonitor.DataAccess.UnitTests
{
    public class SecretLoaderTest
    {
        private string TestFilePath => "test_SecretLoader.xml";

        private string SchemaDeploymentFilePath => "secrets.xsd";

        private string DeploymentXmlNamespace => "http://www.currencymonitor.com/secrets";

        private string CreateValidXml(string[] innerXmlElements)
        {
            var buffer = new System.Text.StringBuilder();
            foreach (string entry in innerXmlElements)
                buffer.AppendLine(entry);

            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8"" ?><secrets xmlns=""http://www.currencymonitor.com/secrets""><database>{0}</database></secrets>", buffer.ToString());
        }

        [Theory]
        [InlineData("<ill></formed>", typeof(System.Xml.XmlException))]
        [InlineData("<root></root>", typeof(System.Xml.Schema.XmlSchemaException))]
        public void GetDatabaseConnString_WhenInvalidXml_ThenThrow(string xmlContent, Type exceptionType)
        {
            File.WriteAllText(TestFilePath, xmlContent);

            Assert.Throws(exceptionType, () => new SecretLoader(
                new XmlMetadata("http://nichts.de", TestFilePath, SchemaDeploymentFilePath)));

            File.Delete(TestFilePath);
        }

        [Fact]
        public void GetDatabaseConnString_WhenXmlEmpty_ThenDoNotThrow()
        {
            File.WriteAllText(TestFilePath, CreateValidXml(new string[] { "" }));

            new SecretLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            File.Delete(TestFilePath);
        }

        [Fact]
        public void GetDatabaseConnString_WhenNameUnavailable_ThenThrow()
        {
            File.WriteAllText(TestFilePath, CreateValidXml(new string[] {
                @"<connection name=""eins"" server=""Server1"" database=""Datenbank1"" userid=""Benutzer1"" password=""Passwort1"" />"
            }));

            var secretLoader = new SecretLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            Assert.Throws<ApplicationException>(() => secretLoader.GetDatabaseConnString("zwei"));

            File.Delete(TestFilePath);
        }

        [Fact]
        public void GetDatabaseConnString_WhenOneAvailable_ThenGiveIt()
        {
            File.WriteAllText(TestFilePath, CreateValidXml(new string[] {
                @"<connection name=""eins"" server=""Server1"" database=""Datenbank1"" userid=""Benutzer1"" password=""Passwort1"" />"
            }));

            var secretLoader = new SecretLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            Assert.Equal("Server=Server1;Database=Datenbank1;User ID=Benutzer1;Password=Passwort1;", secretLoader.GetDatabaseConnString("eins"));

            File.Delete(TestFilePath);
        }

        [Fact]
        public void GetDatabaseConnString_WhenManyAvailable_ThenGiveThem()
        {
            File.WriteAllText(TestFilePath, CreateValidXml(new string[] {
                @"<connection name=""eins"" server=""Server1"" database=""Datenbank1"" userid=""Benutzer1"" password=""Passwort1"" />",
                @"<connection name=""zwei"" server=""Server2"" database=""Datenbank2"" userid=""Benutzer2"" password=""Passwort2"" />",
                @"<connection name=""drei"" server=""Server3"" database=""Datenbank3"" userid=""Benutzer3"" password=""Passwort3"" />"
            }));

            var secretLoader = new SecretLoader(
                new XmlMetadata(DeploymentXmlNamespace, TestFilePath, SchemaDeploymentFilePath));

            Assert.Equal("Server=Server1;Database=Datenbank1;User ID=Benutzer1;Password=Passwort1;", secretLoader.GetDatabaseConnString("eins"));

            Assert.Equal("Server=Server2;Database=Datenbank2;User ID=Benutzer2;Password=Passwort2;", secretLoader.GetDatabaseConnString("zwei"));

            Assert.Equal("Server=Server3;Database=Datenbank3;User ID=Benutzer3;Password=Passwort3;", secretLoader.GetDatabaseConnString("drei"));

            File.Delete(TestFilePath);
        }

    }// end of class SecretLoaderTest

}// end of namespace CurrencyMonitor.DataAccess.UnitTests
