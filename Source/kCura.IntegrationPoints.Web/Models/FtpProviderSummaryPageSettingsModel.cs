using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

namespace kCura.IntegrationPoints.Web.Models
{
	public class FtpProviderSummaryPageSettingsModel
	{
		public string Host { get; set; }
		public string Port { get; set; }
		public string Protocol { get; set; }
		public string UserName { get; set; }
		public string Password => "******";
		public string FileNamePrefix { get; set; }
		public string TimezoneOffset { get; set; }

		public FtpProviderSummaryPageSettingsModel(Settings settings)
		{
			Host = settings.Host;
			Port = settings.Port.ToString();
			Protocol = settings.Protocol;
			UserName = settings.Username ?? string.Empty;
			FileNamePrefix = settings.Filename_Prefix;
			TimezoneOffset = settings.Timezone_Offset.ToString();
		}
	}
}