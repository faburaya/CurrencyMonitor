using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Reusable.DataAccess;

namespace CurrencyMonitor.DataAccess
{
    /// <summary>
    /// Ersatz für die herkömmliche Beschaffung der Verbindungszeichenfolge.
    /// </summary>
    public class ConnectionStringProvider
    {
        private readonly SecretLoader _secretLoader;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Richtet einen Anbieter für Verbindungszeichenfolgen ein.
        /// </summary>
        /// <param name="secretFilePath">Das Pfad der Datei, die die geheimen Verbindungszeichenfolgen steckt.</param>
        /// <param name="configuration">Gewährt die Einstellungen für die Anwendung.</param>
        public ConnectionStringProvider(string secretFilePath, IConfiguration configuration)
        {
            _secretLoader = null;
            if (File.Exists(secretFilePath))
            {
                _secretLoader = new SecretLoader(
                    new Reusable.DataAccess.Common.XmlMetadata(
                        "http://dataaccess.reusable.faburaya.com/secrets",
                        secretFilePath,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schema", "secrets.xsd"))
                );
            }

            _configuration = configuration;
        }

        /// <remarks>
        /// Die geheime Verbindungszeichenfolge kommt aus einer versteckten XML Datei.
        /// Wenn solche Datei nicht vorhanden ist, holt es die Verbindungszeichenfolge
        /// aus den Einstellungen der Anwendung heraus.
        /// </remarks>
        /// <param name="connectionName">Der Name der Verbindung.</param>
        /// <returns>
        /// Die Verbindugszeichenfolge für die Datenbank im Einsatz,
        /// wenn vorhanden, andernfalls <c>null</c>.
        /// </returns>
        public string GetSecretConnectionString(string connectionName)
        {
            // versucht vorzugsweise die Verbindungszeichenkette aus den Geheimnissen herauszuholen:
            string secret = _secretLoader?.GetDatabaseConnString(connectionName);
            if (secret != null)
            {
                return secret;
            }

            // ... andernfalls greift auf die herkömmliche Einstellungen zurück:
            return _configuration.GetConnectionString(connectionName);
        }

    }// end of class ConnectionStringProvider

}// end of namespace CurrencyMonitor.DataAccess
