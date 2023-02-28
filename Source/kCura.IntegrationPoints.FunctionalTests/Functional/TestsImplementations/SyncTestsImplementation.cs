using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private SyncTestsImplementationTemplate _testImplementation;

        public SyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnTearDownFixture()
        {
            _testImplementation.OnTearDownFixture();
        }

        public void SavedSearchNativesAndMetadataGoldFlow()
        {
            // Arrange
            _testImplementation = new SavedSearchNativesSyncTestsImplementation(_testsImplementationTestFixture);

            _testImplementation.ImportDocuments();

            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void ProductionImagesGoldFlow(YesNo copyFilesToRepository)
        {
            // Arrange
            _testImplementation = new ProductionImagesSyncTestsImplementation(_testsImplementationTestFixture, copyFilesToRepository);

            _testImplementation.ImportDocuments();

            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void EntitiesPushGoldFlow()
        {
            // Arrange
            _testImplementation = new EntitiesPushSyncTestsImplementation(_testsImplementationTestFixture);

            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }
    }
}
