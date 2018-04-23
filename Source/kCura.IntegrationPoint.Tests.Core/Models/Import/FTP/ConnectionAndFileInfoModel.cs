using System.Security;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.FTP
{
	public class ConnectionAndFileInfoModel
	{
		private SecureString _secureUsername = new SecureString();

		private SecureString _securePassword = new SecureString();

		public string Host { get; set; }

		public FtpProtocolType Protocol { get; set; }

		public int Port { get; set; }

		public string Username
		{
			get => _secureUsername.ToPlainString();
			set
			{
				_secureUsername = new SecureString();
				value.ForEach(c => _secureUsername.AppendChar(c));
			}
		}

		public string Password
		{
			get => _securePassword.ToPlainString();
			set
			{
				_securePassword = new SecureString();
				value.ForEach(c => _securePassword.AppendChar(c));
			}
		}

		public string CsvFilepath { get; set; }
	}
}