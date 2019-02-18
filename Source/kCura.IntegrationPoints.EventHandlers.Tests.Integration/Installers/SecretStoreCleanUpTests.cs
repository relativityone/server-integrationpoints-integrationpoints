using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations;
using NUnit.Framework;
using Relativity.Core;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	public class SecretStoreCleanUpTests : SourceProviderTemplate
	{
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;
		private SecretStoreCleanUp _secretStoreCleanUp;

		public SecretStoreCleanUpTests() : base($"secret_clean_{Utils.FormattedDateTimeNow}")
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();
			_secretCatalog = SecretStoreFactory.GetSecretStore(BaseServiceContextHelper.Create().GetMasterRdgContext());
			_secretManager = new SecretManager(WorkspaceArtifactId);

			var secret = _secretManager.GenerateIdentifier();

			_secretCatalog.WriteSecret(secret, new Dictionary<string, string> {{"secret_key", "secret_value"}});

			_secretStoreCleanUp = new SecretStoreCleanUp(_secretManager, _secretCatalog);
		}

        [Test]
		[TestInQuarantine(TestQuarantineState.DetectsDefectInExternalDependency, 
					@"Known Issue in secret store: 
					whole paths from secret catalog are returned instead 
					of keys in dictionary. Details: STVD-12542")]
		public void ItShouldRemoveSecretAndTenantId()
		{
			string tenantId = _secretManager.GetTenantID();
			int secretsInitialCount = _secretCatalog.GetTenantSecrets(tenantId).Count;

			_secretStoreCleanUp.CleanUpSecretStore();

			int secretsCount = _secretCatalog.GetTenantSecrets(tenantId).Count;
			Assert.That(secretsInitialCount, Is.Not.EqualTo(0));
			Assert.That(secretsCount, Is.EqualTo(0));
		}
	}
}