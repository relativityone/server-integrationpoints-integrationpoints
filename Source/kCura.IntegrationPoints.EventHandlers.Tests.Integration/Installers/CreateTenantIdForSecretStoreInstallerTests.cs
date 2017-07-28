using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
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
			IEHContext context = new EHContext
			{
				Helper = new EHHelper(Helper, WorkspaceArtifactId)
			};
			var validator = new TenantForSecretStoreCreationValidator(context, new SecretManagerFactory(), new DefaultSecretCatalogFactory());

			// ACT
			var result = validator.Validate();

			// ASSERT
			Assert.That(result, Is.True);
		}
	}
}