using System;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;

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
    }
}
