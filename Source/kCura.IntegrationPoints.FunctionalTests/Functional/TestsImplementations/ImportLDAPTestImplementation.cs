using Atata;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportLDAPTestImplementation
    {
		private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

		public ImportLDAPTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
		{
			_testsImplementationTestFixture = testsImplementationTestFixture;
		}

		public void OnSetUpFixture()
		{
			InstallLegalHoldToWorkspace(_testsImplementationTestFixture.Workspace.ArtifactID);
		}

		public void ImportFromLDAPGoldFlow()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			string integrationPointName = nameof(ImportFromLDAPGoldFlow);

			// Act
			var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			var importFromLDAPConnectToSourcePage = FillOutIntegrationPointEditPageForImportFromLDAP(integrationPointEditPage, integrationPointName);

            var importFromLDAPMapFieldsPage = FillOutImportFromLDAPConnectToSourcePage(importFromLDAPConnectToSourcePage, Const.LDAP.OPEN_LDAP_CONNECTION_PATH, Const.LDAP.OPEN_LDAP_USERNAME, Const.LDAP.OPEN_LDAP_PASSWORD);

			var integrationPointViewPage = SetFieldsMappkingImportFromLDAPMapFieldsPage(importFromLDAPMapFieldsPage);

			integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

			// Assert
			int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
			int workspaceEntityCount = RelativityFacade.Instance.Resolve<IEntityService>().GetAll(_testsImplementationTestFixture.Workspace.ArtifactID).Length;
			transferredItemsCount.Should().Be(workspaceEntityCount).And.Be(13);
		}

		private static void InstallLegalHoldToWorkspace(int workspaceId)
		{
			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();
			applicationService.InstallToWorkspace(workspaceId, applicationService.Get("Relativity Legal Hold").ArtifactID);
		}

		private static ImportFromLDAPConnectToSourcePage FillOutIntegrationPointEditPageForImportFromLDAP(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
		{
			IntegrationPointEdit integrationPointEdit = new IntegrationPointEditImport
			{
				Name = integrationPointName,
				Source = IntegrationPointSources.LDAP,
				TransferredObject = IntegrationPointTransferredObjects.Entity,
			};

			integrationPointEditPage.ApplyModel(integrationPointEdit);

			return integrationPointEditPage.ImportFromLDAPNext.ClickAndGo();
		}

        private static ImportFromLDAPMapFieldsPage FillOutImportFromLDAPConnectToSourcePage(ImportFromLDAPConnectToSourcePage importFromLDAPConnectToSourcePage, string connectionPath, string username, string password)
        {
			return importFromLDAPConnectToSourcePage
			   .ApplyModel(new ImportFromLDAPConnectToSource
			   {
				   ConnectionPath = connectionPath,
				   Username = username,
				   Password = password,
			   }).Next.ClickAndGo();
        }

		private static IntegrationPointViewPage SetFieldsMappkingImportFromLDAPMapFieldsPage(ImportFromLDAPMapFieldsPage importFromLDAPMapFieldsPage)
		{
			importFromLDAPMapFieldsPage.Cn.DoubleClick();
			importFromLDAPMapFieldsPage.GivenName.DoubleClick();
			importFromLDAPMapFieldsPage.Sn.DoubleClick();
			importFromLDAPMapFieldsPage.UniqueID.DoubleClick();
			importFromLDAPMapFieldsPage.FirstName.DoubleClick();
			importFromLDAPMapFieldsPage.LastName.DoubleClick();

			return importFromLDAPMapFieldsPage.Save.ClickAndGo();
		}
	}
}
