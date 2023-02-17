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
        IFtpConnector CreateFtpConnector(string host, int port, string username, string password);

        /// <summary>
        /// /// Returns SFtp Connector instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IFtpConnector CreateSftpConnector(string host, int port, string username, string password);

        /// <summary>
        /// Gets appropriate connector based on parameter
        /// </summary>
        /// <param name="protocolName"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IFtpConnector GetConnector(string protocolName, string host, int port, string username, string password);
    }
}
