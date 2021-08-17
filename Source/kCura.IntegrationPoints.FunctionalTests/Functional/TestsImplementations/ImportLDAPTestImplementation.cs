using Atata;
using System;
using System.IO;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;
using NUnit.Framework;

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

		public void OnTearDownFixture()
		{

		}

		private static void InstallLegalHoldToWorkspace(int workspaceId)
		{
			string rapFileLocation = Path.Combine(TestContext.Parameters["RAPDirectory"], "Relativity_Legal_Hold.rap");

			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

			int appId = applicationService.InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
			{
				IgnoreVersion = true
			});

			applicationService.InstallToWorkspace(workspaceId, appId);
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

            string connectionPath = "rip-openldap-cvnx78s.eastus.azurecontainer.io/ou=Human Resources,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";
            string username = "cn=admin,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";
            string password = "Test1234!";
            var importFromLDAPMapFieldsPage = FillOutImportFromLDAPConnectToSourcePage(importFromLDAPConnectToSourcePage, connectionPath, username, password);

			importFromLDAPMapFieldsPage.Cn.DoubleClick();
			importFromLDAPMapFieldsPage.GivenName.DoubleClick();
			importFromLDAPMapFieldsPage.Sn.DoubleClick();
			importFromLDAPMapFieldsPage.UniqueID.DoubleClick();
			importFromLDAPMapFieldsPage.FirstName.DoubleClick();
			importFromLDAPMapFieldsPage.LastName.DoubleClick();

			var integrationPointViewPage = importFromLDAPMapFieldsPage.Save.ClickAndGo();

			integrationPointViewPage = RunIntegrationPoint(integrationPointViewPage, integrationPointName);

			// Assert
			int transferredItemsCount = GetTransferredItemsCount(integrationPointViewPage, integrationPointName);
			int workspaceEntityCount = RelativityFacade.Instance.Resolve<IEntityService>().GetAll(_testsImplementationTestFixture.Workspace.ArtifactID).Length;
			transferredItemsCount.Should().Be(workspaceEntityCount).And.Be(13);
		}

		private static ImportFromLDAPConnectToSourcePage FillOutIntegrationPointEditPageForImportFromLDAP(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
		{
			IntegrationPointEdit integrationPointEdit = new IntegrationPointEditImport
			{
				Source = IntegrationPointSources.LDAP,
				TransferredObject = IntegrationPointTransferredObjects.Entity
			};
			integrationPointEdit.Name = integrationPointName;
			return integrationPointEditPage.ApplyModel(integrationPointEdit).ImportFromLDAPNext.ClickAndGo();
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

		private static IntegrationPointViewPage RunIntegrationPoint(IntegrationPointViewPage integrationPointViewPage, string integrationPointName)
		{
			return integrationPointViewPage.Run.WaitTo.Within(60).BeVisible().
				Run.ClickAndGo().
				OK.ClickAndGo().
				WaitUntilJobCompleted(integrationPointName);
		}

		private static int GetTransferredItemsCount(IntegrationPointViewPage integrationPointViewPage, string integrationPointName)
		{
			return Int32.Parse(integrationPointViewPage.Status.Table.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
		}
	}
}
