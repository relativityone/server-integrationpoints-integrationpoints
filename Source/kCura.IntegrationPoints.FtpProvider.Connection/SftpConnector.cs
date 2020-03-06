using System;
using System.IO;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Relativity.API;
using Renci.SshNet;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class SftpConnector : Interfaces.IFtpConnector
    {
		private readonly IAPILog _logger;

        internal bool disposed;
        internal SftpClient _sftpClient;
        internal Stream _fileStream;

        internal Int32 _downloadRetryCount;
        internal Int32 _streamRetryCount;
        internal Int32 _timeout;

        public Int32 Timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                _sftpClient.OperationTimeout = new TimeSpan(0, 0, value);
            }
        }

		private SftpConnector(IAPILog logger)
		{
			_logger = logger;
		}

        public SftpConnector(IAPILog logger, String host, Int32 port, String username, String password)
            : this(logger)
        {
            username = String.IsNullOrWhiteSpace(username) ? Constants.DefaultUsername : username.Normalize();
            password = String.IsNullOrWhiteSpace(username) ? Constants.DefaultPassword : password.Normalize();

            ConnectionInfo connection = new ConnectionInfo(host.Normalize(), port, username.Normalize(),
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(username.Normalize(), password.Normalize()),
                }
                );
			LogConnectionInfo(connection);

            _sftpClient = new SftpClient(connection);
            Timeout = Constants.Timeout;
        }

        public bool TestConnection()
        {
			_logger.LogInformation("Connection test has been started...");
            var success = false;
            if (_sftpClient.IsConnected)
            {
				_logger.LogInformation("Client is connected");
				LogConnectionInfo(_sftpClient.ConnectionInfo);
                success = true;
            }
            else
            {
				_logger.LogInformation("Client is trying to connect");
                _sftpClient.Connect();
                if (_sftpClient.IsConnected)
                {
					_logger.LogInformation("Client is connected");
					LogConnectionInfo(_sftpClient.ConnectionInfo);
                    success = true;
                }
            }
            return success;
        }

	    public Stream DownloadStream(String remotePath, String fileName, Int32 retryLimit)
        {
			_logger.LogInformation("Trying to open read stream from file... Attempts {retry}/{totalRetries}.", _streamRetryCount+1, retryLimit);
            Stream stream = null;
            try
            {
                string fullRemotePath = Path.Combine(remotePath, fileName);
                fullRemotePath = FilenameFormatter.FormatFtpPath(fullRemotePath);
                if (TestConnection())
                {
	                _fileStream?.Dispose();
	                _fileStream = _sftpClient.OpenRead(fullRemotePath);
                    stream = _fileStream;
					_logger.LogInformation("Read stream is open.");
                }
            }
            catch (Exception ex)
            {
				_logger.LogInformation(ex, "Exception occured during opening read stream");
                _streamRetryCount++;
                if (_streamRetryCount < retryLimit)
                {
                    stream = DownloadStream(remotePath, fileName, retryLimit);
                }
                else
                {
					_logger.LogError(ex, "No more retries. Exception has been thrown");
                    throw;
                }
            }
            finally
            {
                _streamRetryCount = 0;
            }

            return stream;
        }

        public void DownloadFile(String localDownloadPath, String remotePath, String fileName, Int32 retryLimit)
        {
            try
            {
				_logger.LogInformation("Trying to download file... Attempts {retry}/{totalRetries}.", _streamRetryCount+1, retryLimit);
                string fullRemotePath = Path.Combine(remotePath, fileName);
                fullRemotePath = FilenameFormatter.FormatFtpPath(fullRemotePath);
                string fullLocalPath = localDownloadPath;
                if (TestConnection())
                {
                    using (FileStream fs = File.OpenWrite(fullLocalPath))
                    {
                        _sftpClient.DownloadFile(fullRemotePath, fs);
						_logger.LogInformation("File has been sucessfully downloaded.");
                    }
                }
            }
            catch (Exception ex)
            {
				_logger.LogInformation(ex, "Exception occured during downloading file");
                _downloadRetryCount++;
                if (_downloadRetryCount < retryLimit)
                {
                    DownloadFile(localDownloadPath, remotePath, fileName, retryLimit);
                }
                else
                {
					_logger.LogError(ex, "No more retries. Exception has been thrown");
                    throw;
                }
            }
            finally
            {
                _downloadRetryCount = 0;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
	                _sftpClient?.Disconnect();
					_fileStream?.Dispose();
	                if (_sftpClient != null)
                    {
                        if (_sftpClient.IsConnected)
                        {
                            _sftpClient.Disconnect();
                        }
                        _sftpClient.Dispose();
                    }
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

		private void LogConnectionInfo(ConnectionInfo connection)
		{
			_logger.LogInformation("ConnectionInfo - Host: {host}, Port: {port}, Encoding: {encoding}, Timeout: {timeout}, IsAuthenticated: {isAuthenticated}, RetryAttemps: {retries}",
				connection.Host, connection.Port, connection.Encoding, connection.Timeout, connection.IsAuthenticated, connection.RetryAttempts);
			_logger.LogInformation("ProxyInfo - ProxyHost: {proxyHost}, ProxyPort: {proxyPort}, ProxyType: {proxyType}",
				connection.ProxyHost, connection.ProxyPort, connection.ProxyType);
			_logger.LogInformation("ClientInfo - ClientVersion: {clientVersion}, CurrentClientCompressionAlgorithm: {compressionAlgorithm}, " 
				+ "CurrentClientEncryption: {clientEncryption}, CurrentClientHmacAlgorithm: {clientHmacAlgorithm}",
				connection.ClientVersion, connection.CurrentClientCompressionAlgorithm,
				connection.CurrentClientEncryption, connection.CurrentClientHmacAlgorithm);
			_logger.LogInformation("ServerInfo - ServerVersion: {serverversion}, CurrentServerComppressionAlgorithm: {compressionAlgorithm}, "
				+ "CurrentServerEncryption: {serverEncryption}, CurrentServerHmacAlgorithm: {serverHmacAlgorithm}",
				connection.ServerVersion, connection.CurrentServerCompressionAlgorithm, connection.CurrentServerEncryption, connection.CurrentServerHmacAlgorithm);
		}
	}
}
