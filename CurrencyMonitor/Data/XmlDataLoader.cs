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

        private static XmlDocument ParseXmlFile(File file)
        {
            var dom = new XmlDocument();

            switch (file)
            {
                case File.InitialData:
                    dom.Load(System.IO.Path.Combine("Data", "InitializationData.xml"));
                    break;

                default:
                    throw new NotSupportedException($"XML-Datei {file} ist nicht zum Laden unterstützt!");
            }

            return dom;
        }


        private XmlDocument _dom;
        private Func<XmlDocument> _delayedParseXml;
        private XmlDocument XmlDataSource
        {
            get { return _dom ??= _delayedParseXml(); }
        }

        public XmlDataLoader(File file)
        {
            _delayedParseXml = (() => ParseXmlFile(file));
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

            const string xpath = "/deployment/currencies/entry";
            foreach (XmlNode node in XmlDataSource.SelectNodes(xpath))
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