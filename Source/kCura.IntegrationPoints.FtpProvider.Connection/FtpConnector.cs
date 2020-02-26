using System;
using System.IO;
using System.Net;
using System.Text;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
	public class FtpConnector : Interfaces.IFtpConnector
    {
		private readonly IAPILog _logger;

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

		private FtpConnector(IAPILog logger)
		{
			_logger = logger;
		}

        public FtpConnector(IAPILog logger, string host, int port, string username, string password)
			: this(logger)
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
			_logger.LogInformation("Connection test has been started...");
	        _request = CreateRequest(BuildBaseFtpUrl(_host, _port), _username, _password);
	        _request.Method = WebRequestMethods.Ftp.ListDirectory;

			_logger.LogInformation("Client is trying to connect");
            using (var connectionTest = (FtpWebResponse)_request.GetResponse())
            {
                connectionTest.Close();
				_logger.LogInformation("Connection test passed");
            }
            return true;
        }

	    public Stream DownloadStream(string remotePath, string fileName, int retryLimit)
        {
	        try
	        {
				_logger.LogInformation("Trying to download read stream from file... Attempts {retry}/{totalRetries}.", _streamRetryCount + 1, retryLimit);
				if (TestConnection())
				{
					_request = CreateRequest(BuildDownloadUrl(remotePath, fileName), _username, _password);
					_request.Method = WebRequestMethods.Ftp.DownloadFile;
			        _ftpClient = (FtpWebResponse)_request.GetResponse();
			        _stream = new FtpStream(_ftpClient.GetResponseStream(), _request);
					_logger.LogInformation("Stream downloaded successfully.");
				}
	        }
	        catch (Exception ex)
	        {
				_logger.LogInformation(ex, "Exception occured during downloading stream");
		        _streamRetryCount++;
		        if (_streamRetryCount < retryLimit)
		        {
			        _stream = DownloadStream(remotePath, fileName, retryLimit);
		        }
		        else
		        {
					_logger.LogError(ex, "No more retries. Exception has been thrown");
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
				_logger.LogInformation("Trying to download file... Attempts {retry}/{totalRetries}.", _streamRetryCount + 1, retryLimit);
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
									_logger.LogInformation("FileStream has been created.");
                                    var buffer = new byte[BufferSize];

                                    var readCount = responseStream.Read(buffer, 0, BufferSize);
                                    while (readCount > 0)
                                    {
                                        writer.Write(buffer, 0, readCount);
                                        readCount = responseStream.Read(buffer, 0, BufferSize);
                                    }
                                }
								_logger.LogInformation("File has been sucessfully downloaded.");
                            }
                        }
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
