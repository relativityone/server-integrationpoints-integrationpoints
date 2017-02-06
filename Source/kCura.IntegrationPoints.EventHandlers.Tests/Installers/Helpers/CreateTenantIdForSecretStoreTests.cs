using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers.Helpers
{
	public class CreateTenantIdForSecretStoreTests : TestBase
	{
		private ISecretManager _secretManager;
		private ISecretCatalog _secretCatalog;
		private CreateTenantIdForSecretStore _createTenantIdForSecretStore;

		public override void SetUp()
		{
			_secretManager = Substitute.For<ISecretManager>();
			_secretCatalog = Substitute.For<ISecretCatalog>();
			_createTenantIdForSecretStore = new CreateTenantIdForSecretStore(_secretCatalog, _secretManager);
		}

		[Test]
		public void ItShouldCreateTenantId()
		{
			string tenantId = "expectedTenantId";
			_secretManager.GetTenantID().Returns(tenantId);

			_createTenantIdForSecretStore.Create();

			_secretManager.Received(1).GetTenantID();
			_secretCatalog.Received(1).CreateTenantEncryptionSecret(tenantId);
		}
	}
}