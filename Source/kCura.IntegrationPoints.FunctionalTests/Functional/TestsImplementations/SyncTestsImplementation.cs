using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncTestsImplementation
    {
        private SyncTestsImplementationTemplate _testImplementation;

        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

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
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void ProductionImagesGoldFlow(YesNo copyFilesToRepository)
        {
            // Arrange
            _testImplementation = new ProductionImagesSyncTestsImplemention(_testsImplementationTestFixture, copyFilesToRepository);
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }

        public void EntitiesPushGoldFlow()
        {
            // Arrange
            _testImplementation = new EntitiesPushSyncTestsImplemention(_testsImplementationTestFixture);
            IntegrationPointViewPage integrationPointViewPage = _testImplementation.CreateIntegrationPointViewPage();

            // Act
            _testImplementation.RunIntegrationPoint(integrationPointViewPage);

            // Assert
            _testImplementation.AssertIntegrationPointJobHistory(integrationPointViewPage);
        }
    }
}
