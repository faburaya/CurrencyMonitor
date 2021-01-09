using System;
using System.Xml;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyMonitor.Data
{
    /// <summary>
    /// Lädt Daten aus einer XML datei und speichert sie.
    /// </summary>
    public class XmlDataLoader
    {
        public enum File { InitialData }

        /// <summary>
        /// Bescreibt weitere Informationen über eine XML-Datei.
        /// </summary>
        private class FileMetadata
        {
            public string XmlFilePath { get; }

            public string SchemaFilePath { get; }

            public string XmlNamespace { get; }

            public FileMetadata(string directory,
                                string xmlFileName,
                                string schemaFileName,
                                string xmlNamespace)
            {
                this.XmlFilePath = System.IO.Path.Combine(directory, xmlFileName);
                this.SchemaFilePath = System.IO.Path.Combine(directory, schemaFileName);
                this.XmlNamespace = xmlNamespace;
            }
        }

        private static FileMetadata GetMetadata(File file)
        {
            switch (file)
            {
                case File.InitialData:
                    return new FileMetadata("Data",
                                            "deployment.xml",
                                            "deployment.xsd",
                                            "http://www.currencymonitor.com/deployment");
                default:
                    throw new NotSupportedException($"XML-Datei {file} ist noch nicht unterstützt!");
            }
        }

        private static XmlDocument ParseXmlFile(FileMetadata metadata)
        {
            var dom = new XmlDocument();
            dom.Load(metadata.XmlFilePath);
            dom.Schemas.Add(metadata.XmlNamespace, metadata.SchemaFilePath);
            dom.Validate(null);
            return dom;
        }

        private FileMetadata _metadata;
        private XmlDocument _dom;
        private XmlDocument XmlDataSource
        {
            get { return _dom ??= ParseXmlFile(_metadata); }
        }

        public XmlDataLoader(File file)
        {
            _metadata = GetMetadata(file);
        }

        public enum DataSet { Currencies }

        /// <summary>
        /// Lädt die Daten aus der XML-Datei und speichert sie durch EF.
        /// </summary>
        /// <param name="dataSet">Welche Daten geladen werden müssen.</param>
        /// <param name="dbContext">Die Datenkontextklasse, wo die Daten gespeichert werden.</param>
        public void Load(DataSet dataSet, IServiceProvider serviceProvider)
        {
            using CurrencyMonitorContext dbContext = new CurrencyMonitorContext(
                serviceProvider.GetRequiredService<DbContextOptions<CurrencyMonitorContext>>());

            switch (dataSet)
            {
                case DataSet.Currencies:
                    LoadCurrencies(dbContext.RecognizedCurrency);
                    break;

                default:
                    throw new NotSupportedException($"DataSet {dataSet} ist nicht zum Laden unterstützt!");
            }

            dbContext.SaveChanges();
        }

        private void LoadCurrencies(DbSet<DataModels.RecognizedCurrency> intoDataSet)
        {
            if (intoDataSet.Any())
                return;

            XmlNamespaceManager nsManager = new XmlNamespaceManager(XmlDataSource.NameTable);
            nsManager.AddNamespace("tns", _metadata.XmlNamespace);

            const string xpath = "/tns:deployment/tns:currencies/tns:entry";
            foreach (XmlNode node in XmlDataSource.SelectNodes(xpath, nsManager))
            {
                var entry = node as XmlElement;

                var deserializedObject = new DataModels.RecognizedCurrency
                {
                    Code = entry.GetAttribute("code"),
                    Name = entry.GetAttribute("name"),
                    Symbol = entry.GetAttribute("symbol"),
                    Country = entry.GetAttribute("country")
                };

                intoDataSet.Add(deserializedObject);
            }
        }

    } // end of class XmlDataLoader
}// end of namespace CurrencyMonitor.Data