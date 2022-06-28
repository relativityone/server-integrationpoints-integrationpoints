using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();

        public SyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
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

        public void SavedSearchNativesAndMetadataGoldFlow()
        {
            // Arrange
            SavedSearchNativesSyncTestsImplementation testImplementation = new SavedSearchNativesSyncTestsImplementation(_testsImplementationTestFixture);
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Act
            testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void ProductionImagesGoldFlow(YesNo copyFilesToRepository)
        {
            // Arrange
            ProductionImagesSyncTestsImplemention testImplementation = new ProductionImagesSyncTestsImplemention(_testsImplementationTestFixture, copyFilesToRepository);
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Act
            testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void EntitiesPushGoldFlow()
        {
            // Arrange
            EntitiesPushSyncTestsImplemention testImplementation = new EntitiesPushSyncTestsImplemention(_testsImplementationTestFixture);
            IntegrationPointViewPage integrationPointViewPage = testImplementation.CreateIntegrationPointViewPage();

            // Act
            testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }
    }
}
