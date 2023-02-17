using System;
using System.IO;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Relativity.API;
using Renci.SshNet;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class SftpConnector : IFtpConnector
    {
        private bool _disposed;
        private int _downloadRetryCount;
        private int _streamRetryCount;
        private int _timeout = Constants.Timeout;
        private Stream _fileStream;
        private readonly SftpClient _sftpClient;
        private readonly string _host;
        private readonly IHostValidator _hostValidator;
        private readonly IAPILog _logger;

        public int Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                _sftpClient.OperationTimeout = new TimeSpan(0, 0, value);
            }
        }

        public SftpConnector(string host, int port, string username, string password, IHostValidator hostValidator, IAPILog logger)
        {
            _host = host;
            _hostValidator = hostValidator;
            _logger = logger;

            username = string.IsNullOrWhiteSpace(username) ? Constants.DefaultUsername : username.Normalize();
            password = string.IsNullOrWhiteSpace(username) ? Constants.DefaultPassword : password.Normalize();

            ConnectionInfo connection = new ConnectionInfo(host.Normalize(), port, username.Normalize(),
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(username.Normalize(), password.Normalize()),
                });
            _sftpClient = new SftpClient(connection);

            LogConnectionInfo(connection);
        }

        public bool TestConnection()
        {
            _logger.LogInformation("Checking if can connect to specified host");
            if (!_hostValidator.CanConnectTo(_host))
            {
                _logger.LogInformation("Cannot connect to specified host because it's blacklisted");
                return false;
            }

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

        public Stream DownloadStream(string remotePath, string fileName, int retryLimit)
        {
            _logger.LogInformation("Trying to open read stream from file... Attempts {retry}/{totalRetries}.", _streamRetryCount + 1, retryLimit);
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

        public void DownloadFile(string localDownloadPath, string remotePath, string fileName, int retryLimit)
        {
            try
            {
                _logger.LogInformation("Trying to download file... Attempts {retry}/{totalRetries}.", _streamRetryCount + 1, retryLimit);
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
            if (!_disposed)
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
            _disposed = true;
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
