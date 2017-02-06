using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.SecretStore
{
	public class SecretManagerTests : TestBase
	{
		private const int _WORKSPACE_ARTIFACT_ID = 622465;
		private SecretManager _secretManager;

		public override void SetUp()
		{
			_secretManager = new SecretManager(_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldGenerateNewIdentifier()
		{
			var secret = _secretManager.GenerateIdentifier();

			Assert.That(!string.IsNullOrWhiteSpace(secret.SecretID));
			Assert.That(secret.TenantID, Is.EqualTo(GetTenantId()));
		}

		[Test]
		public void ItShouldRetrieveIdentifierForRdo()
		{
			var expectedSecretId = "518310";
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = expectedSecretId
			};
			var secret = _secretManager.RetrieveIdentifier(rdo);

			Assert.That(secret.SecretID, Is.EqualTo(expectedSecretId));
			Assert.That(secret.TenantID, Is.EqualTo(GetTenantId()));
		}

		[Test]
		public void ItShouldRetrieveIdentifier()
		{
			var securedConfiguration = "518310";
			var secret = _secretManager.RetrieveIdentifier(securedConfiguration);

			Assert.That(secret.SecretID, Is.EqualTo(securedConfiguration));
			Assert.That(secret.TenantID, Is.EqualTo(GetTenantId()));
		}

		[Test(Description = "We're assuming that empty SecuredConfiguration exists only in old Integration Points")]
		[TestCase(null)]
		[TestCase(" ")]
		[TestCase("")]
		public void ItShouldHandleEmptySecuredConfigurationWhileRetrieving(string securedConfiguration)
		{
			var secret = _secretManager.RetrieveIdentifier(securedConfiguration);

			Assert.That(!string.IsNullOrWhiteSpace(secret.SecretID));
			Assert.That(secret.TenantID, Is.EqualTo(GetTenantId()));
		}

		[Test]
		[TestCase(null)]
		[TestCase(" ")]
		[TestCase("")]
		public void ItShouldHandleEmptySecuredConfigurationWhileRetrievingForRdo(string securedConfiguration)
		{
			var secret = _secretManager.RetrieveIdentifier(new IntegrationPoint {SecuredConfiguration = securedConfiguration});

			Assert.That(!string.IsNullOrWhiteSpace(secret.SecretID));
			Assert.That(secret.TenantID, Is.EqualTo(GetTenantId()));
		}

		[Test]
		public void ItShouldRetrieveValue()
		{
			string expectedValue = "expectedValue_110";

			var dict = new Dictionary<string, string> {[nameof(IntegrationPoint.SecuredConfiguration)] = expectedValue};

			var actualValue = _secretManager.RetrieveValue(dict);

			Assert.That(actualValue, Is.EqualTo(expectedValue));
		}

		[Test]
		public void ItShouldCreateSecretData()
		{
			string valueToSecure = "value_537";

			var secretData = _secretManager.CreateSecretData(new IntegrationPoint {SecuredConfiguration = valueToSecure});

			Assert.That(secretData.ContainsKey(nameof(IntegrationPoint.SecuredConfiguration)));
			Assert.That(secretData[nameof(IntegrationPoint.SecuredConfiguration)], Is.EqualTo(valueToSecure));
		}

		[Test]
		public void ItShouldReturnTenantId()
		{
			var tenantId = _secretManager.GetTenantID();

			Assert.That(tenantId, Is.EqualTo(GetTenantId()));
		}

		private string GetTenantId()
		{
			return $"{SecretStoreConstants.TENANT_ID_PREFIX}:{_WORKSPACE_ARTIFACT_ID}";
		}
	}
}