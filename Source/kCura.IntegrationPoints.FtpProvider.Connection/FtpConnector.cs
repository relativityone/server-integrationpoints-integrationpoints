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
	    internal FtpWebRequest _request;
		internal FtpWebResponse _ftpClient;
        internal Stream _stream;

        internal readonly string _host;
        internal readonly int _port;
        internal readonly string _username;
        internal readonly string _password;
        internal int _downloadRetryCount;
        internal int _streamRetryCount;
        public int Timeout { get; set; }
        public int BufferSize { get; set; }

        public FtpConnector(string host, int port, string username, string password)
        {
            _host = host.Normalize();
            _port = port;
            _username = string.IsNullOrWhiteSpace(username) ? Constants.DefaultUsername : username.Normalize();
            _password = string.IsNullOrWhiteSpace(username) ? Constants.DefaultPassword : password.Normalize();
            BufferSize = 2048;
            Timeout = Constants.Timeout;
        }

        public bool TestConnection()
        {
	        _request = CreateRequest(BuildBaseFtpUrl(_host, _port), _username, _password);
	        _request.Method = WebRequestMethods.Ftp.ListDirectory;
            using (var connectionTest = (FtpWebResponse)_request.GetResponse())
            {
                connectionTest.Close();
            }
            return true;
        }

	    public Stream DownloadStream(string remotePath, string fileName, int retryLimit)
        {
	        try
	        {
		        if (TestConnection())
				{
					_request = CreateRequest(BuildDownloadUrl(remotePath, fileName), _username, _password);
					_request.Method = WebRequestMethods.Ftp.DownloadFile;
			        _ftpClient = (FtpWebResponse)_request.GetResponse();
			        _stream = new FtpStream(_ftpClient.GetResponseStream(), _request);
		        }
	        }
	        catch (Exception)
	        {
		        _streamRetryCount++;
		        if (_streamRetryCount < retryLimit)
		        {
			        _stream = DownloadStream(remotePath, fileName, retryLimit);
		        }
		        else
		        {
			        throw;
		        }
	        }
	        _streamRetryCount = 0;
	        return _stream;
		}

        public void DownloadFile(string localDownloadPath, string remotePath, string fileName, int retryLimit)
        {
            try
            {
                if (TestConnection())
                {
	                _request = CreateRequest(BuildDownloadUrl(remotePath, fileName), _username, _password);
	                _request.Method = WebRequestMethods.Ftp.DownloadFile;

                    using (var response = (FtpWebResponse)_request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (var writer = new FileStream(localDownloadPath, FileMode.Create))
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

        internal FtpWebRequest CreateRequest(string url, string username, string password)
        {
	        var request = (FtpWebRequest)WebRequest.Create(url);
            request.Timeout = Timeout * 1000;
            PrepareCredentials(request, username, password);
            return request;
        }

        internal string BuildDownloadUrl(string remotePath, string filename)
        {
            var sb = new StringBuilder();
            sb.Append(BuildBaseFtpUrl(_host, _port));
            sb.Append(Path.Combine(remotePath, filename));
            return sb.ToString().Replace("\\", "/");
        }

        internal void PrepareCredentials(WebRequest ftpClient, string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                ftpClient.Credentials = new NetworkCredential(username, password);
            }
        }

        internal string BuildBaseFtpUrl(string host, int port)
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
	                _stream?.Dispose();
	                _ftpClient?.Dispose();
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
