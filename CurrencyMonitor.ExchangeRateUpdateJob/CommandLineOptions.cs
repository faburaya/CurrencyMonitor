using CommandLine;

namespace CurrencyMonitor.ExchangeRateUpdateJob
{
    internal class CommandLineOptions
    {
        [Option(shortName: 'd', longName: "database", Required = true, HelpText = "Name der Datenbank")]
        public string DatabaseName { get; set; }

        [Option(shortName: 's', longName: "connectionstring", Required = true, HelpText = "Verbindungszeichenfolge")]
        public string ConnectionString { get; set; }
    }
}
