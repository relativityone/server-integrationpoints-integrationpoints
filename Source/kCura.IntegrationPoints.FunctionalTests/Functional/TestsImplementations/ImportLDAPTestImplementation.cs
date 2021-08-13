using Atata;
using System;
using System.IO;
using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
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
			//not sure if its needed?
			RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());

			InstallLegalHoldToWorkspace(_testsImplementationTestFixture.Workspace.ArtifactID);
		}

		public void OnTearDownFixture()
		{
			//leave empty for now
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

            //string connectionPath = "";
            //string username = "";
            //string password = "";
            //var importFromLDAPMapFieldsPage = FillOutImportFromLDAPConnectToSourcePage(importFromLDAPConnectToSourcePage, connectionPath, username, password);

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
    }
}
