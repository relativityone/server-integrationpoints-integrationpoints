using System.Security;

namespace kCura.IntegrationPoint.Tests.Core.Models.FTP
{
	public class ImportFromFTPConnectionAndFileInfoModel
	{
		public string Host { get; set; }
		public FTPProtocolType Protocol { get; set; }
		public string Port { get; set; }
		public SecureString Username { get; set; }
		public SecureString Password { get; set; }
		public string CSVFilepath { get; set; }
	}
}
