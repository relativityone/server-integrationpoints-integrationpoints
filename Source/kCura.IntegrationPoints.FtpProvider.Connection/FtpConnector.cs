using System;
using System.IO;
using System.Net;
using System.Text;
using kCura.IntegrationPoints.FtpProvider.Helpers;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class FtpConnector : Interfaces.IFtpConnector
    {
        internal bool _disposed;
        internal FtpWebResponse _ftpClient;
        internal Stream _fileStream;

        internal readonly String _host;
        internal readonly Int32 _port;
        internal readonly String _username;
        internal readonly String _password;
        internal Int32 _downloadRetryCount;
        internal Int32 _streamRetryCount;
        public Int32 Timeout { get; set; }
        public Int32 BufferSize { get; set; }

        public FtpConnector(String host, Int32 port, String username, String password)
        {
            _host = host.Normalize();
            _port = port;
            _username = String.IsNullOrWhiteSpace(username) ? Constants.DefaultUsername : username.Normalize();
            _password = String.IsNullOrWhiteSpace(username) ? Constants.DefaultPassword : password.Normalize();
            BufferSize = 2048;
            Timeout = Constants.Timeout;
        }

        public bool TestConnection()
        {
            var ftpClient = GetClient(BuildBaseFtpUrl(_host, _port), _username, _password);
            ftpClient.Method = WebRequestMethods.Ftp.ListDirectory;
            using (var connectionTest = (FtpWebResponse)ftpClient.GetResponse())
            {
                connectionTest.Close();
            }
            return true;
        }

        public Stream DownloadStream(String remotePath, String fileName, Int32 retryLimit)
        {
            Stream retVal = null;
            try
            {
                if (TestConnection())
                {
                    var ftpClient = GetClient(BuildDownloadUrl(remotePath, fileName), _username, _password);
                    ftpClient.Method = WebRequestMethods.Ftp.DownloadFile;
                    _ftpClient = (FtpWebResponse)ftpClient.GetResponse();
                    _fileStream = _ftpClient.GetResponseStream();
                    retVal = _fileStream;
                }
            }
            catch (Exception)
            {
                _streamRetryCount++;
                if (_streamRetryCount < retryLimit)
                {
                    retVal = DownloadStream(remotePath, fileName, retryLimit);
                }
                else
                {
                    throw;
                }
            }
            _streamRetryCount = 0;
            return retVal;
        }

        public void DownloadFile(String localDownloadPath, String remotePath, String fileName, Int32 retryLimit)
        {
            try
            {
                if (TestConnection())
                {
                    var fullLocalPath = localDownloadPath;
                    var ftpClient = GetClient(BuildDownloadUrl(remotePath, fileName), _username, _password);
                    ftpClient.Method = WebRequestMethods.Ftp.DownloadFile;

                    using (var response = (FtpWebResponse)ftpClient.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (var writer = new FileStream(fullLocalPath, FileMode.Create))
                                {
                                    var buffer = new byte[BufferSize];

                                    var readCount = responseStream.Read(buffer, 0, BufferSize);
                                    while (readCount > 0)
                                    {
                                        writer.Write(buffer, 0, readCount);
                                        readCount = responseStream.Read(buffer, 0, BufferSize);
                                    }
                                }
                            }
                        }
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
            _downloadRetryCount = 0;
        }

        internal FtpWebRequest GetClient(String url, String username, String password)
        {
            var retVal = (FtpWebRequest)WebRequest.Create(url);
            retVal.Timeout = Timeout * 1000;
            PrepareCredentials(retVal, username, password);
            return retVal;
        }

        internal String BuildDownloadUrl(String remotePath, String filename)
        {
            var sb = new StringBuilder();
            sb.Append(BuildBaseFtpUrl(_host, _port));
            sb.Append(Path.Combine(remotePath, filename));
            return sb.ToString().Replace("\\", "/");
        }

        internal void PrepareCredentials(WebRequest ftpClient, String username, String password)
        {
            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                ftpClient.Credentials = new NetworkCredential(username, password);
            }
        }

        internal String BuildBaseFtpUrl(String host, Int32 port)
        {
            var sb = new StringBuilder();
            sb.Append("ftp://");
            sb.Append(host);
            sb.Append(":" + port);
            return sb.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_fileStream != null)
                    {
                        _fileStream.Dispose();
                    }
                    if (_ftpClient != null)
                    {
                        _ftpClient.Dispose();
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
    }
}
