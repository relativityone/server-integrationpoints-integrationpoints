using kCura.IntegrationPoints.EventHandlers.Commands;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class RemoveSecuredConfigurationFromIntegrationPointServiceTests
    {
        private const string SourceConfigurationWithSecuredConfiguration =
            @"{""FederatedInstanceArtifactId"":1,""SavedSearchArtifactId"":""1"",""SecuredConfiguration"":""{\""ClientId\"":\""1886\"",\""ClientSecret\"":\""8edf\""}"",""SourceWorkspaceArtifactId"":""8""}";
        private const string DestinationConfigurationWithSecuredConfiguration =
            @"{""artifactTypeID"":10,""destinationProviderType"":""74A863B9-00EC-4BB7-9B3E-1E22323010C6"",""SecuredConfiguration"":""{\""ClientId\"":\""1886\"",\""ClientSecret\"":\""8edf\""}"",""CreateSavedSearchForTagging"":""false"",""ProductionImport"":false}";
        private const string SourceConfigurationWithoutSecuredConfiguration =
            @"{""FederatedInstanceArtifactId"":1,""SavedSearchArtifactId"":""1"",""SourceWorkspaceArtifactId"":""8""}";
        private const string DestinationConfigurationWithoutSecuredConfiguration =
            @"{""artifactTypeID"":10,""destinationProviderType"":""74A863B9-00EC-4BB7-9B3E-1E22323010C6"",""CreateSavedSearchForTagging"":""false"",""ProductionImport"":false}";

        [Test]
        public void ItShouldReturnFalseWhenNullOrEmptyStringIsPresent()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = null,
                DestinationConfiguration = string.Empty
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ItShouldNotThrowWhenNullIsGiven()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();

            bool result = service.RemoveSecuredConfiguration(null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ItShouldKeepOriginalStringsWhenInvalidJsonIsPresent()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = "notJSON",
                DestinationConfiguration = "notANotherJSON"
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.False);
            Assert.That(integrationPoint.SourceConfiguration, Is.EqualTo("notJSON"));
            Assert.That(integrationPoint.DestinationConfiguration, Is.EqualTo("notANotherJSON"));
        }

        [Test]
        public void ItShouldUpdateOnePropertyEvenIfANotherIsInvalid()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = "notJSON",
                DestinationConfiguration = DestinationConfigurationWithSecuredConfiguration
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.True);
            Assert.That(integrationPoint.SourceConfiguration, Is.EqualTo("notJSON"));
            Assert.That(integrationPoint.DestinationConfiguration, Is.EqualTo(DestinationConfigurationWithoutSecuredConfiguration));
        }

        [Test]
        public void ItShouldRemoveSecuredConfigurationFromBothProperties()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = SourceConfigurationWithSecuredConfiguration,
                DestinationConfiguration = DestinationConfigurationWithSecuredConfiguration
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.True);
            Assert.That(integrationPoint.SourceConfiguration, Is.EqualTo(SourceConfigurationWithoutSecuredConfiguration));
            Assert.That(integrationPoint.DestinationConfiguration, Is.EqualTo(DestinationConfigurationWithoutSecuredConfiguration));
        }

        [Test]
        public void ItShouldReturnFalseWhenNoSecuredConfigurationIsPresent()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = SourceConfigurationWithoutSecuredConfiguration,
                DestinationConfiguration = DestinationConfigurationWithoutSecuredConfiguration
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ItShouldKeepOriginalConfigurationWhenNoSecuredConfigurationIsPresent()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = SourceConfigurationWithoutSecuredConfiguration,
                DestinationConfiguration = DestinationConfigurationWithoutSecuredConfiguration
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.False);
            Assert.That(integrationPoint.SourceConfiguration, Is.EqualTo(SourceConfigurationWithoutSecuredConfiguration));
            Assert.That(integrationPoint.DestinationConfiguration, Is.EqualTo(DestinationConfigurationWithoutSecuredConfiguration));
        }

        [Test]
        public void ItShouldReturnTrueIfAnyPropertyContainsSecuredConfiguration()
        {
            var service = new RemoveSecuredConfigurationFromIntegrationPointService();
            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = SourceConfigurationWithSecuredConfiguration,
                DestinationConfiguration = "InvalidJSON"
            };

            bool result = service.RemoveSecuredConfiguration(integrationPoint);

            Assert.That(result, Is.True);
            Assert.That(integrationPoint.SourceConfiguration, Is.EqualTo(SourceConfigurationWithoutSecuredConfiguration));
            Assert.That(integrationPoint.DestinationConfiguration, Is.EqualTo("InvalidJSON"));
        }
    }
}
