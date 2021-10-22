namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ImportFromFTPConnectToSource
    {
        public string Host { get; set; }

        public FTPProtocol Protocol { get; set; }

        public decimal? Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string CSVFilePath { get; set; }
    }
}
