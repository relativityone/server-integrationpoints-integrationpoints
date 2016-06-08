using System;
using System.IO;
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

            _sftpClient = new SftpClient(host.Normalize(), port, username.Normalize(), password.Normalize());
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
            Stream retVal = null;
            try
            {
                var fullRemotePath = Path.Combine(remotePath, fileName);
                if (TestConnection())
                {
                    if (_fileStream != null)
                    {
                        _fileStream.Dispose();
                    }
                    _fileStream = _sftpClient.OpenRead(fullRemotePath);
                    retVal = _fileStream;
                }
            }
            catch (Exception)
            {
                _streamRetryCount++;
                if (_streamRetryCount < retryLimit)
                {
                    DownloadStream(remotePath, fileName, retryLimit);
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

            return retVal;
        }

        public void DownloadFile(String localDownloadPath, String remotePath, String fileName, Int32 retryLimit)
        {
            try
            {
                var fullRemotePath = Path.Combine(remotePath, fileName);
                var fullLocalPath = localDownloadPath;
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
                    if (_fileStream != null)
                    {
                        _fileStream.Dispose();
                    }
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
