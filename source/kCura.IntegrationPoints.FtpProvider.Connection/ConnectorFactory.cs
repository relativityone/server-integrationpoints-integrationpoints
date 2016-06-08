using System;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class ConnectorFactory : IConnectorFactory
    {
        public IFtpConnector CreateFtpConnector(String host, Int32 port, String username, String password)
        {
            return new FtpConnector(host, port, username, password);
        }

        public IFtpConnector CreateSftpConnector(String host, Int32 port, String username, String password)
        {
            return new SftpConnector(host, port, username, password);
        }

        public IFtpConnector GetConnector(string protocolName, String host, Int32 port, String username, String password)
        {
            IFtpConnector client = null;

            if (protocolName.Equals(ProtocolName.FTP))
            {
                client = this.CreateFtpConnector(host, port, username, password);
            }
            else if (protocolName.Equals(ProtocolName.SFTP))
            {
                client = this.CreateSftpConnector(host, port, username, password);
            }

            return client;
        }
    }
}
