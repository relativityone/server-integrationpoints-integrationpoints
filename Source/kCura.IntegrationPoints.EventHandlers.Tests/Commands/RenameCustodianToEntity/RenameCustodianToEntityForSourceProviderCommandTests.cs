using System.Collections.Generic;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.RenameCustodianToEntity
{
    [TestFixture, Category("Unit")]
    public class RenameCustodianToEntityForSourceProviderCommandTests : UpdateConfigurationCommandTestsBase
    {
        private RenameCustodianToEntityForSourceProviderCommand _sut;

        private const string _CONFIGURATION_WITHOUT_CUSTODIAN_PROPERTY = "{\"PropertyA\":\"value\"}";
        private const string _CONFIGURATION_WITH_CUSTODIAN_PROPERTY = "{\"PropertyA\":\"value\",\"CustodianManagerFieldContainsLink\":\"v2\"}";
        private const string _CONFIGURATION_WITH_ENTITY_PROPERTY = "{\"PropertyA\":\"value\",\"EntityManagerFieldContainsLink\":\"v2\"}";

        protected override List<string> Names => new List<string> { "Destination Configuration" };

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _sut = new RenameCustodianToEntityForSourceProviderCommand(It.IsAny<string>(), EHHelperFake.Object, RelativityObjectManagerMock.Object);
        }

        [Test]
        public void Execute_ShouldNotProcess_WhenDestinationConfigurationIsNull()
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(null);
            
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }


        [Test]
        public void Execute_ShouldNotProcess_WhenConfigurationHasNotBeenChanged()
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(_CONFIGURATION_WITHOUT_CUSTODIAN_PROPERTY);

            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        [Test]
        public void Execute_ShouldProcess_WhenPropertyInConfigurationHasBeenRenamed()
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(_CONFIGURATION_WITH_CUSTODIAN_PROPERTY);
            RelativityObjectSlim objectSlimExpected = PrepareObject(_CONFIGURATION_WITH_ENTITY_PROPERTY);

            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldBeUpdated(objectSlimExpected);
        }

        private RelativityObjectSlim PrepareObject(string destinationConfiguration)
        {
            return new RelativityObjectSlim()
            {
                ArtifactID = 1,
                Values = new List<object>()
                {
                    destinationConfiguration
                }
            };
        }
    }
}
