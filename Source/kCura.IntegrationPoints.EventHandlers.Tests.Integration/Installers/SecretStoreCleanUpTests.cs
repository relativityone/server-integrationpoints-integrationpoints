using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Templates;
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

		public SecretStoreCleanUpTests() : base($"secret_clean_{Utils.FormatedDateTimeNow}")
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
		public void ItShouldRemoveSecretAndTenantId()
		{
			_secretStoreCleanUp.CleanUpSecretStore();

			Thread.Sleep(1000);

			var sqlStatement = $"SELECT COUNT(*) FROM [SQLSecretStore] WHERE [TenantID] = '{_secretManager.GetTenantID()}'";
			var secretCount = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(sqlStatement);

			Assert.That(secretCount, Is.EqualTo(0));
		}
	}
}