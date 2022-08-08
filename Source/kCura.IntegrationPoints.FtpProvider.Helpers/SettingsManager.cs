using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public class SettingsManager : ISettingsManager
    {
        public Settings DeserializeSettings(string jsonString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(jsonString);
        }

        public SecuredConfiguration DeserializeCredentials(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return new SecuredConfiguration();
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<SecuredConfiguration>(jsonString);
        }
    }
}
