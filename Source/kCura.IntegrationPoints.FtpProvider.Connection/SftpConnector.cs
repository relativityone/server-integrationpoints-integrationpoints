using System;
using System.IO;
using System.Net;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Renci.SshNet;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class SftpConnector : Interfaces.IFtpConnector
    {
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

        public SftpConnector(String host, Int32 port, String username, String password)
        {
            username = String.IsNullOrWhiteSpace(username) ? Constants.DefaultUsername : username.Normalize();
            password = String.IsNullOrWhiteSpace(username) ? Constants.DefaultPassword : password.Normalize();

            // Setup Credentials and Server Information 
            ConnectionInfo ConnNfo = new ConnectionInfo(host.Normalize(), port, username.Normalize(),
                new AuthenticationMethod[]
                {
                    // Pasword based Authentication 
                    new PasswordAuthenticationMethod(username.Normalize(), password.Normalize()),
                }
                );

            _sftpClient = new SftpClient(ConnNfo);
            Timeout = Constants.Timeout;
        }

        public bool TestConnection()
        {
            var success = false;
            if (_sftpClient.IsConnected)
            {
                success = true;
            }
            else
            {
                _sftpClient.Connect();
                if (_sftpClient.IsConnected)
                {
                    success = true;
                }
            }
            return success;
        }

	    public Stream DownloadStream(String remotePath, String fileName, Int32 retryLimit)
        {
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
                }
            }
            catch (Exception)
            {
                _streamRetryCount++;
                if (_streamRetryCount < retryLimit)
                {
                    stream = DownloadStream(remotePath, fileName, retryLimit);
                }
                else
                {
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
                string fullRemotePath = Path.Combine(remotePath, fileName);
                fullRemotePath = FilenameFormatter.FormatFtpPath(fullRemotePath);
                string fullLocalPath = localDownloadPath;
                if (TestConnection())
                {
                    using (FileStream fs = File.OpenWrite(fullLocalPath))
                    {
                        _sftpClient.DownloadFile(fullRemotePath, fs);
                    }
                }
            }
            catch (Exception)
            {
                _downloadRetryCount++;
                if (_downloadRetryCount < retryLimit)
                {
                    DownloadFile(localDownloadPath, remotePath, fileName, retryLimit);
                }
                else
                {
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
    }
}
