using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class ConnectorFactory : IConnectorFactory
    {
        private readonly IHostValidator _hostValidator;
        private readonly IAPILog _logger;

        public ConnectorFactory(IHostValidator hostValidator, IHelper helper)
        {
            _hostValidator = hostValidator;
            _logger = helper.GetLoggerFactory().GetLogger();
        }

        public IFtpConnector CreateFtpConnector(string host, int port, string username, string password)
        {
            return new FtpConnector(host, port, username, password, _hostValidator, _logger);
        }

        public IFtpConnector CreateSftpConnector(string host, int port, string username, string password)
        {
            return new SftpConnector(host, port, username, password, _hostValidator, _logger);
        }

        public IFtpConnector GetConnector(string protocolName, string host, int port, string username, string password)
        {
            IFtpConnector client = null;
            _logger.LogInformation("Creating {protocolName} Connector from {host}:{port}.", protocolName, host, port);

            if (protocolName.Equals(ProtocolName.FTP))
            {
                client = CreateFtpConnector(host, port, username, password);
            }
            else if (protocolName.Equals(ProtocolName.SFTP))
            {
                client = CreateSftpConnector(host, port, username, password);
            }

            _logger.LogInformation("{protocolName} Connector was successfully created.", protocolName);
            return client;
        }
    }
}
