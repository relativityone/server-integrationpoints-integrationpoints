using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces
{
    public interface ISettingsManager
    {
        Settings DeserializeSettings(string jsonString);
        SecuredConfiguration DeserializeCredentials(string jsonString);
    }
}
