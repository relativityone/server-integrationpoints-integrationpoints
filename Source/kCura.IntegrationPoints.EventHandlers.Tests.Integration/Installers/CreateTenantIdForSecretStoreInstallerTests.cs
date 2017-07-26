using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.SecretStore;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	public class CreateTenantIdForSecretStoreInstallerTests : SourceProviderTemplate
	{
		public CreateTenantIdForSecretStoreInstallerTests() : base($"TenantID_{Utils.FormatedDateTimeNow}")
		{
		}

		[Test(Description = "This test is to verify that Tenant ID is created during installation")]
		public void ItShouldCreatedTenantIDDuringInstallation()
		{
			//var tenantId = new SecretManager(WorkspaceArtifactId).GetTenantID();
			//var secretId = $"tenant{tenantId}encryptionkey";

			//var sqlStatement = $"SELECT COUNT(*) FROM [SQLSecretStore] WHERE [TenantID] = '{tenantId}' AND [SecretID] = '{secretId}'";
			//var tenantCount = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(sqlStatement);
			//Assert.That(tenantCount, Is.EqualTo(1));
		}
	}
}