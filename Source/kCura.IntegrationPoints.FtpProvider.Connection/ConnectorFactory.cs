using System;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class ConnectorFactory : IConnectorFactory
    {
		private readonly IAPILog _logger;

		public ConnectorFactory(IHelper helper)
		{

			_logger = helper.GetLoggerFactory().GetLogger();
		}

        public IFtpConnector CreateFtpConnector(String host, Int32 port, String username, String password)
        {
            return new FtpConnector(_logger, host, port, username, password);
        }

        public IFtpConnector CreateSftpConnector(String host, Int32 port, String username, String password)
        {
            return new SftpConnector(_logger, host, port, username, password);
        }

        public IFtpConnector GetConnector(string protocolName, String host, Int32 port, String username, String password)
        {
            IFtpConnector client = null;
			_logger.LogInformation("Creating {protocolName} Connector from {host}:{port}.", protocolName, host, port);

            if (protocolName.Equals(ProtocolName.FTP))
            {
                client = this.CreateFtpConnector(host, port, username, password);
            }
            else if (protocolName.Equals(ProtocolName.SFTP))
            {
                client = this.CreateSftpConnector(host, port, username, password);
            }

			_logger.LogInformation("{protocolName} Connector was successfully created.", protocolName);
            return client;
        }
    }
}
