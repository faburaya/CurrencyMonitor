using System.Xml;
using System.Collections.Generic;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Lädt geheime Daten, die außerhalb der appsettings.json bleiben sollen.
    /// (Zum Beispiel, die Verbindungszeichenkette der Datenbank.)
    /// </summary>
    public class SecretLoader
    {
        /// <summary>
        /// Lädt die Datenquelle mit den Geheimnissen.
        /// </summary>
        public SecretLoader(XmlMetadata metadata)
        {
            var dom = new XmlDocument();
            dom.Load(metadata.FilePath);
            dom.Schemas.Add(metadata.XmlNamespace, metadata.SchemaFilePath);
            dom.Validate(null);

            _dbConnStringsByName = LoadDatabaseConnectionStrings(dom, metadata.XmlNamespace);
        }

        private readonly Dictionary<string, string> _dbConnStringsByName;

        /// <summary>
        /// Bietet eine Verbindugszeichenkette für Datenbank.
        /// </summary>
        /// <param name="name">Der Name der Verbindung.</param>
        /// <returns>Der geheime Teil der Verbindungszeichenkette: "Server;Database;User ID;Password;"</returns>
        public string GetDatabaseConnString(string name)
        {
            return _dbConnStringsByName.TryGetValue(name, out string connectionString) ? connectionString : "[Verbindungszeichenkette der Datenbank nicht gefunden!];";
        }

        private static Dictionary<string, string> LoadDatabaseConnectionStrings(XmlDocument dom,
                                                                                string targetNamespace)
        {
            var dbConnStringsByName = new Dictionary<string, string>();

            XmlNamespaceManager nsManager = new XmlNamespaceManager(dom.NameTable);
            nsManager.AddNamespace("tns", targetNamespace);

            const string xpath = "/tns:secrets/tns:database/tns:connection";
            foreach (XmlNode node in dom.SelectNodes(xpath, nsManager))
            {
                var entry = node as XmlElement;

                string connectionName = entry.GetAttribute("name");
                string connectionString = $"Server={entry.GetAttribute("server")};Database={entry.GetAttribute("database")};User ID={entry.GetAttribute("userid")};Password={entry.GetAttribute("password")};";

                dbConnStringsByName.Add(connectionName, connectionString);
            }

            return dbConnStringsByName;
        }

    }// end of class SecretLoader

}// end of namespace CurrencyMonitor.DataAccess
