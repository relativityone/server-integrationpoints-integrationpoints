using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.Helpers
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

			var workspaceId = 798243;
			var helper = Substitute.For<IEHHelper>();
			helper.GetActiveCaseID().Returns(workspaceId);
			var context = new EHContext
			{
				Helper = helper
			};
			var secretManagerFactory = Substitute.For<ISecretManagerFactory>();
			secretManagerFactory.Create(workspaceId).Returns(_secretManager);
			var secretCatalogFactory = Substitute.For<ISecretCatalogFactory>();
			secretCatalogFactory.Create(workspaceId).Returns(_secretCatalog);

			_createTenantIdForSecretStore = new CreateTenantIdForSecretStore(context, secretCatalogFactory, secretManagerFactory);
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