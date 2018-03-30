namespace kCura.IntegrationPoint.Tests.Core.Models.FTP
{
	public class ImportFromFTPConnectionAndFileInfoModel
	{
		public string Host { get; set; }
		public FTPProtocolType Protocol { get; set; }
		public string Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string CSVFilepath { get; set; }
	}
}
