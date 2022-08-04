using kCura.IntegrationPoints.Synchronizers.RDO.Model.Serializer;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Model.Serializer
{
    [TestFixture, Category("Unit")]
    public class ImportSettingsForLogSerializerTests
    {
        [Test]
        public void RelativityPasswordIsNotPresent()
        {
            const string relativityPassword = "SuperSecretPassword";
            var settings = new ImportSettings
            {
                RelativityPassword = relativityPassword
            };

            var serializer = new ImportSettingsForLogSerializer();

            string serializedSettings = serializer.Serialize(settings);
            bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, relativityPassword);

            Assert.IsFalse(isSensitivaDataPresent);
        }

        [Test]
        public void RelativityUsernameIsNotPresent()
        {
            const string relativityUsername = "relativity.admin@relativity.com";
            var settings = new ImportSettings
            {
                RelativityUsername = relativityUsername
            };

            var serializer = new ImportSettingsForLogSerializer();

            string serializedSettings = serializer.Serialize(settings);
            bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, relativityUsername);

            Assert.IsFalse(isSensitivaDataPresent);
        }

        [Test]
        public void FederatedInstanceCredentialsIsNotPresent()
        {
            const string federatedInstanceCredentials = "sensitivaData";
            var settings = new ImportSettings
            {
                FederatedInstanceCredentials = federatedInstanceCredentials
            };

            var serializer = new ImportSettingsForLogSerializer();

            string serializedSettings = serializer.Serialize(settings);
            bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, federatedInstanceCredentials);

            Assert.IsFalse(isSensitivaDataPresent);
        }

        [Test]
        public void OnBehalfOfUserIdIsNotPresent()
        {
            const int onBehalfOfUserId = 487;
            var settings = new ImportSettings
            {
                OnBehalfOfUserId = onBehalfOfUserId
            };

            var serializer = new ImportSettingsForLogSerializer();

            string serializedSettings = serializer.Serialize(settings);
            bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, onBehalfOfUserId.ToString());

            Assert.IsFalse(isSensitivaDataPresent);
        }
        [Test]
        public void OnBehalfOfUserTokenIsNotPresent()
        {
            const string userToken = "userToken";
            const int userId = 2;
            var settings = new ImportSettings
            {
                OnBehalfOfUserId = userId
            };

            var serializer = new ImportSettingsForLogSerializer();

            string serializedSettings = serializer.Serialize(settings);
            bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, userToken);

            Assert.IsFalse(isSensitivaDataPresent);
        }

        [Test]
        public void ImportSettingsHasSpecificNumberOfFields()
        {
            // please verify if ImportSettings properties modification doesnt affect serialization for logging (sensitive data, etc.)
            // if it is ok, just update this number
            const int expectedNumberOfProperties = 66;
            int actualNumberOfProperties = typeof(ImportSettings).GetProperties().Length;

            Assert.AreEqual(expectedNumberOfProperties, actualNumberOfProperties);
        }

        private bool IsSubstringPresentCaseInsensitive(string fullText, string substring)
        {
            string fullTextAsLower = fullText.ToLower();
            string substringAsLower = substring.ToLower();

            return fullTextAsLower.Contains(substringAsLower);
        }
    }
}
