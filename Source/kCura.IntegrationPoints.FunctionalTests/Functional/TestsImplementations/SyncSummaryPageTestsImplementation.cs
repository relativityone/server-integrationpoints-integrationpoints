using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Models;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncSummaryPageTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();

        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public void OnTearDownFixture()
        {
            foreach (KeyValuePair<string, Workspace> destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
            }
        }

        public void SavedSearchNativesAndMetadataSummaryPage()
        {
            // Arrange
            SavedSearchNativesSyncTestsImplementation testImplementation = new SavedSearchNativesSyncTestsImplementation(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Assert
            testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void SavedSearchImagesSummaryPage()
        {
            // Arrange
            SavedSearchImagesSyncTestsImplementation testImplementation = new SavedSearchImagesSyncTestsImplementation(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Assert
            testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void ProductionImagesSummaryPage()
        {
            // Arrange
            ProductionImagesSyncTestsImplemention testImplementation = new ProductionImagesSyncTestsImplemention(_testsImplementationTestFixture, YesNo.Yes);

            // Act
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Assert
            testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void EntitiesPushSummaryPage()
        {
            // Arrange
            EntitiesPushSyncTestsImplemention testImplementation = new EntitiesPushSyncTestsImplemention(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Assert
            testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }
    }
}