using System;
using System.IO;

namespace kCura.IntegrationPoints.FtpProvider.Connection.Interfaces
{
    public interface IFtpConnector : IDisposable
    {
        /// <summary>
        /// Specify the connection Timeout
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Throws exception if unable to connect.
        /// The exception should detail why the user is able to connect.
        /// Bad credetails, unable to reach address...etc
        /// </summary>
        /// <returns>Boolean value of true when connection is successful</returns>
        bool TestConnection();

        /// <summary>
        /// Download file to specified location.
        /// </summary>
        /// <param name="localDownloadPath">The path where the file will be downloaded, include filename</param>
        /// <param name="remotePath">The remote path.  Do not include the filename here</param>
        /// <param name="fileName">The name of the file on the remote server</param>
        /// <param name="retryLimit">Number of time to retry when errors occur</param>
        void DownloadFile(string localDownloadPath, string remotePath, string fileName, int retryLimit);

        /// <summary>
        /// Returns a file stream of the specified file on the remote server
        /// </summary>
        /// <param name="remotePath">The remote path.  Do not include the filename here</param>
        /// <param name="fileName">The name of the file on the remote server</param>
        /// <param name="retryLimit">Number of time to retry when errors occur</param>
        Stream DownloadStream(string remotePath, string fileName, int retryLimit);
    }
}
