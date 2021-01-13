﻿using System;
using System.Xml;
using System.Linq;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Lädt Daten aus einer XML datei und speichert sie.
    /// </summary>
    public class XmlDataLoader
    {
        private static XmlDocument ParseXmlFile(XmlMetadata metadata)
        {
            var dom = new XmlDocument();
            dom.Load(metadata.FilePath);
            dom.Schemas.Add(metadata.XmlNamespace, metadata.SchemaFilePath);
            dom.Validate(null);
            return dom;
        }

        private XmlMetadata _metadata;
        private XmlDocument _dom;
        private XmlDocument XmlDataSource
        {
            get { return _dom ??= ParseXmlFile(_metadata); }
        }

        /// <summary>
        /// Stellt eine Instanz von <see cref="XmlDataLoader"/> her.
        /// </summary>
        /// <param name="metadata">Die Metadaten über das XML, das geladen werden muss.</param>
        public XmlDataLoader(XmlMetadata metadata)
        {
            _metadata = metadata;
        }

        public enum DataSet { Currencies }

        /// <summary>
        /// Lädt die Daten aus der XML-Datei und speichert sie durch EF.
        /// </summary>
        /// <param name="dataSet">Welche Daten geladen werden müssen.</param>
        /// <param name="dbAccess">Gewährt Zugang auf Datenbank.</param>
        public void Load(DataSet dataSet, IDatabaseAccess dbAccess)
        {
            switch (dataSet)
            {
                case DataSet.Currencies:
                    LoadCurrencies(dbAccess);
                    break;

                default:
                    throw new NotSupportedException($"DataSet {dataSet} ist nicht zum Laden unterstützt!");
            }

            dbAccess.Commit();
        }

        private void LoadCurrencies(IDatabaseAccess dbAccess)
        {
            if (dbAccess.HasAny<DataModels.RecognizedCurrency>())
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

                dbAccess.Insert(deserializedObject);
            }
        }

    } // end of class XmlDataLoader
}// end of namespace CurrencyMonitor.DataAccess