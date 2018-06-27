using kCura.IntegrationPoints.Synchronizers.RDO.Model.Serializer;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Model.Serializer
{
	public class ImportSettingsForLogSerializerTests
	{
		[Test]
		public void RelativityPasswordIsNotPresent()
		{
			const string RELATIVITY_PASSWORD = "SuperSecretPassword";
			var settings = new ImportSettings
			{
				RelativityPassword = RELATIVITY_PASSWORD
			};

			var serializer = new ImportSettingsForLogSerializer();

			string serializedSettings = serializer.Serialize(settings);
			bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, RELATIVITY_PASSWORD);

			Assert.IsFalse(isSensitivaDataPresent);
		}

		[Test]
		public void RelativityUsernameIsNotPresent()
		{
			const string RELATIVITY_USERNAME = "relativity.admin@relativity.com";
			var settings = new ImportSettings
			{
				RelativityUsername = RELATIVITY_USERNAME
			};

			var serializer = new ImportSettingsForLogSerializer();

			string serializedSettings = serializer.Serialize(settings);
			bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, RELATIVITY_USERNAME);

			Assert.IsFalse(isSensitivaDataPresent);
		}

		[Test]
		public void FederatedInstanceCredentialsIsNotPresent()
		{
			const string FEDERATED_INSTANCE_CREDENTIALS = "sensitivaData";
			var settings = new ImportSettings
			{
				FederatedInstanceCredentials = FEDERATED_INSTANCE_CREDENTIALS
			};

			var serializer = new ImportSettingsForLogSerializer();

			string serializedSettings = serializer.Serialize(settings);
			bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, FEDERATED_INSTANCE_CREDENTIALS);

			Assert.IsFalse(isSensitivaDataPresent);
		}

		[Test]
		public void OnBehalfOfUserIdIsNotPresent()
		{
			const int ON_BEHALF_OF_USER_ID = 487;
			var settings = new ImportSettings
			{
				OnBehalfOfUserId = ON_BEHALF_OF_USER_ID
			};

			var serializer = new ImportSettingsForLogSerializer();

			string serializedSettings = serializer.Serialize(settings);
			bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, ON_BEHALF_OF_USER_ID.ToString());

			Assert.IsFalse(isSensitivaDataPresent);
		}
		[Test]
		public void OnBehalfOfUserTokenIsNotPresent()
		{
			const string USER_TOKEN = "userToken";
			const int USER_ID = 2;
			var tokenGenerator = Substitute.For<IAuditSpoofTokenGenerator>();
			tokenGenerator.Create(Arg.Any<BaseServiceContext>(), USER_ID).Returns(USER_TOKEN);

			var settings = new ImportSettings(tokenGenerator, null)
			{
				OnBehalfOfUserId = USER_ID
			};

			var serializer = new ImportSettingsForLogSerializer();

			string serializedSettings = serializer.Serialize(settings);
			bool isSensitivaDataPresent = IsSubstringPresentCaseInsensitive(serializedSettings, USER_TOKEN);

			Assert.IsFalse(isSensitivaDataPresent);
		}

		[Test]
		public void ImportSettingsHasSpecificNumberOfFields()
		{
			// please verify if ImportSettings properties modification doesnt affect serialization for logging (sensitive data, etc.)
			// if it is ok, just update this number
			const int expectedNumberOfProperties = 64;
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
