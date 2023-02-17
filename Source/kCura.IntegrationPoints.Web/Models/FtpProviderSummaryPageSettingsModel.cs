using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

namespace kCura.IntegrationPoints.Web.Models
{
    public class FtpProviderSummaryPageSettingsModel
    {
        public string Host { get; set; }

        public string Port { get; set; }

        public string Protocol { get; set; }

        public string FileNamePrefix { get; set; }

        public string TimezoneOffset { get; set; }

        public FtpProviderSummaryPageSettingsModel(Settings settings)
        {
            Host = settings.Host;
            Port = settings.Port.ToString();
            Protocol = settings.Protocol;
            FileNamePrefix = settings.Filename_Prefix;
            TimezoneOffset = settings.Timezone_Offset.ToString();
        }
    }
}
