using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers.Helpers
{
	public class SecretStoreCleanUpTests : TestBase
	{
		private SecretStoreCleanUp _secretStoreCleanUp;
		private ISecretManager _secretManager;
		private ISecretCatalog _secretCatalog;

		public override void SetUp()
		{
			_secretManager = Substitute.For<ISecretManager>();
			_secretCatalog = Substitute.For<ISecretCatalog>();
			_secretStoreCleanUp = new SecretStoreCleanUp(_secretManager, _secretCatalog);
		}

		[Test]
		public void ItShouldRemoveSecrets()
		{
			var tenantId = "tenant_322";
			var firstSecret = new Dictionary<string, Dictionary<string, string>> {{"first_secret_711", null}};
			var secondSecret = new Dictionary<string, Dictionary<string, string>> {{"second_secret_404", null}};
			var secrets = new List<Dictionary<string, Dictionary<string, string>>> {firstSecret, secondSecret};

			_secretManager.GetTenantID().Returns(tenantId);
			_secretCatalog.GetTenantSecrets(tenantId).Returns(secrets);

			_secretStoreCleanUp.CleanUpSecretStore();

			_secretManager.Received().GetTenantID();
			_secretCatalog.Received(1).GetTenantSecrets(tenantId);
			_secretCatalog.Received(1).RevokeSecretAsync(Arg.Is<SecretRef>(x => x.SecretID == "first_secret_711" && x.TenantID == tenantId));
			_secretCatalog.Received(1).RevokeSecretAsync(Arg.Is<SecretRef>(x => x.SecretID == "second_secret_404" && x.TenantID == tenantId));
		}

		[Test]
		public void ItShouldRemoveTenant()
		{
			var tenantId = "tenant_226";
			var expectedSecretId = $"tenant{tenantId}encryptionkey";

			_secretManager.GetTenantID().Returns(tenantId);
			_secretCatalog.GetTenantSecrets(tenantId).Returns(new List<Dictionary<string, Dictionary<string, string>>>());

			_secretStoreCleanUp.CleanUpSecretStore();

			_secretManager.Received().GetTenantID();
			_secretCatalog.Received(1).RevokeSecretAsync(Arg.Is<SecretRef>(x => x.SecretID == expectedSecretId && x.TenantID == tenantId));
		}
	}
}