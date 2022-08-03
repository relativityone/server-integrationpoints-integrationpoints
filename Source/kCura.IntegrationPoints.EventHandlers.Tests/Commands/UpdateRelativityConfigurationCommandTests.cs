using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class UpdateRelativityConfigurationCommandTests : UpdateConfigurationCommandTestsBase
    {
        private const string _SOURCE_CONFIGURATION_WITH_SECURED = @"{""FederatedInstanceArtifactId"":1, ""SecuredConfiguration"":""{\""ClientId\"":\""1886\"",\""ClientSecret\"":\""8edf\""}""}";
        private const string _DESTINATION_CONFIGURATION_WITH_SECURED = @"{""artifactTypeID"":106, ""SecuredConfiguration"":""{\""ClientId\"":\""1886\"",\""ClientSecret\"":\""8edf\""}""}";
        private const string _SOURCE_CONFIGURATION_WITHOUT_SECURED = @"{""FederatedInstanceArtifactId"":1}";
        private const string _DESTINATION_CONFIGURATION_WITHOUT_SECURED = @"{""artifactTypeID"":106}";

        private UpdateRelativityConfigurationCommand _sut;

        protected override List<string> Names => new List<string>() { "Secured Configuration", "Source Configuration", "Destination Configuration" };

        public override void SetUp()
        {
            base.SetUp();

            _sut = new UpdateRelativityConfigurationCommand(EHHelperFake.Object, RelativityObjectManagerMock.Object);
        }

        [Test]
        public void Execute_ShouldNotProcess_WhenSecuredConfigurationIsNull()
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration: "");
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }


        [TestCase(_SOURCE_CONFIGURATION_WITH_SECURED, "", _SOURCE_CONFIGURATION_WITHOUT_SECURED, "")]
        [TestCase("", _DESTINATION_CONFIGURATION_WITH_SECURED, "", _DESTINATION_CONFIGURATION_WITHOUT_SECURED)]
        [TestCase(_SOURCE_CONFIGURATION_WITH_SECURED, _DESTINATION_CONFIGURATION_WITH_SECURED, _SOURCE_CONFIGURATION_WITHOUT_SECURED, _DESTINATION_CONFIGURATION_WITHOUT_SECURED)]
        public void Execute_ShouldProcess_WhenOneOfTheConfigurationsHasBeenUpdated(string sourceConfiguration, string destinationConfiguration,
            string expectedSourceConfiguration, string expectedDestinationConfiguration)
        {
            // Arrange
            const string securedConfiguration = "Some Configuration";

            RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration, 
                sourceConfiguration, destinationConfiguration);

            RelativityObjectSlim objectSlimExpected = PrepareObject(securedConfiguration,
                expectedSourceConfiguration, expectedDestinationConfiguration);
            
            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldBeUpdated(objectSlimExpected);
        }

        public void Execute_ShouldNotProcess_WhenNothingHasBeenUpdated()
        {
            // Arrange
            const string securedConfiguration = "Some Configuration";

            RelativityObjectSlim objectSlim = PrepareObject(securedConfiguration,
                _SOURCE_CONFIGURATION_WITHOUT_SECURED, _DESTINATION_CONFIGURATION_WITHOUT_SECURED);

            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        private RelativityObjectSlim PrepareObject(string securedConfiguration = null, string sourceConfiguration = null,
                string destinationConfiguration = null)
        {
            {
                return new RelativityObjectSlim()
                {
                    ArtifactID = 1,
                    Values = new List<object>()
                    {
                        securedConfiguration,
                        sourceConfiguration,
                        destinationConfiguration
                    }
                };
            }
        }
    }
}