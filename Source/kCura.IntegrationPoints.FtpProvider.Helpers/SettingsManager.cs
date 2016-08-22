using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public class SettingsManager : ISettingsManager
    {
        private IEncryptionManager _encryptionManager;

        public SettingsManager(IEncryptionManager encryptionManager)
        {
            _encryptionManager = encryptionManager;
        }

        public Settings ConvertFromString(string data)
        {
            Settings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(data);
            return settings;
        }

        public Settings ConvertFromEncryptedString(string encryptedData)
        {
            var decrptedData = _encryptionManager.Decrypt(encryptedData);
            return ConvertFromString(decrptedData);
        }
    }
}
