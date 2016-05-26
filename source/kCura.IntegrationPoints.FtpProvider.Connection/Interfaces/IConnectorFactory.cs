using System;

namespace kCura.IntegrationPoints.FtpProvider.Connection.Interfaces
{
    public interface IConnectorFactory 
    {
        /// <summary>
        /// Returns Ftp Connector instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IFtpConnector CreateFtpConnector(String host, Int32 port, String username, String password);

        /// <summary>
        /// /// Returns SFtp Connector instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IFtpConnector CreateSftpConnector(String host, Int32 port, String username, String password);
    }
}
