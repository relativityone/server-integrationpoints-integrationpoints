using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncSummaryPageTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        private SyncTestsImplementationTemplate _testImplementation;

        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnTearDownFixture()
        {
            _testImplementation.OnTearDownFixture();
        }

        public void SavedSearchNativesAndMetadataSummaryPage()
        {
            // Arrange
            _testImplementation = new SavedSearchNativesSyncTestsImplementation(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Assert
            _testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void SavedSearchImagesSummaryPage()
        {
            // Arrange
            _testImplementation = new SavedSearchImagesSyncTestsImplementation(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Assert
            _testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void ProductionImagesSummaryPage()
        {
            // Arrange
            _testImplementation = new ProductionImagesSyncTestsImplementation(_testsImplementationTestFixture, YesNo.Yes);

            _testImplementation.ImportDocuments();

            // Act
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Assert
            _testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }

        public void EntitiesPushSummaryPage()
        {
            // Arrange
            _testImplementation = new EntitiesPushSyncTestsImplementation(_testsImplementationTestFixture);

            // Act
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Assert
            _testImplementation.AssertIntegrationPointSummaryPageGeneralTab(integrationPointViewPage);
        }
    }
}
