using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncSummaryPageTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            this._testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void SavedSearchNativesAndMetadataSummaryPage()
        {
            // Arrange
            SavedSearchNativesAndMetadataSyncTestsImplementation testImplementation = new SavedSearchNativesAndMetadataSyncTestsImplementation(_testsImplementationTestFixture);

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