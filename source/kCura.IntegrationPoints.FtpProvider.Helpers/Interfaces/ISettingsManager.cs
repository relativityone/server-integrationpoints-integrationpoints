using kCura.IntegrationPoints.FtpProvider.Helpers.Models;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces
{
    public interface ISettingsManager
    {
        Settings ConvertFromString(string data);
        Settings ConvertFromEncryptedString(string encryptedData);
    }
}
