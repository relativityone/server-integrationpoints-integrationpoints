using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class UpdateDestinationConfigurationTaggingSettingsCommandTests : UpdateConfigurationCommandTestsBase
    {
        private UpdateDestinationConfigurationTaggingSettingsCommand _sut;

        protected override List<string> Names => new List<string> { "Destination Configuration" };

        public override void SetUp()
        {
            base.SetUp();
            _sut = new UpdateDestinationConfigurationTaggingSettingsCommand(EHHelperFake.Object, RelativityObjectManagerMock.Object);
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

        [TestCase(@"{""artifactTypeID"":10,""ImportOverwriteMode"":""AppendOverlay"",""ImagePrecedence"":[],""ProductionPrecedence"":0,""TaggingOption"":""Enabled""}")]
        [TestCase(@"{""artifactTypeID"":10,""ImportOverwriteMode"":""AppendOverlay"",""ImagePrecedence"":[],""ProductionPrecedence"":0}")]
        public void Execute_ShouldNotProcess_WhenEnableTaggingPropertyIsNotSet(string destinationConfiguration)
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(destinationConfiguration);

            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldNotBeUpdated();
        }

        [TestCase(@"{""artifactTypeID"":10,""EnableTagging"":""true""}", @"{""artifactTypeID"":10,""TaggingOption"":""Enabled""}")]
        [TestCase(@"{""artifactTypeID"":10,""EnableTagging"":true}", @"{""artifactTypeID"":10,""TaggingOption"":""Enabled""}")]
        [TestCase(@"{""artifactTypeID"":10,""EnableTagging"":false}", @"{""artifactTypeID"":10,""TaggingOption"":""Disabled""}")]
        [TestCase(@"{""artifactTypeID"":10,""EnableTagging"":""false""}", @"{""artifactTypeID"":10,""TaggingOption"":""Disabled""}")]
        [TestCase(@"{""artifactTypeID"":10,""EnableTagging"":""false"", ""TaggingOption"":""Enabled""}", @"{""artifactTypeID"":10,""TaggingOption"":""Disabled""}")]
        public void Execute_ShouldProcess_WhenObsoleteTaggingSettingExists(string inputJson, string expectedResult)
        {
            // Arrange
            RelativityObjectSlim objectSlim = PrepareObject(inputJson);
            RelativityObjectSlim objectSlimExpected = PrepareObject(expectedResult);

            SetupRead(objectSlim);

            // Act
            _sut.Execute();

            // Assert
            ShouldBeUpdated(objectSlimExpected);
        }

        protected override void ShouldBeUpdated(RelativityObjectSlim objectSlim)
        {
            const int expectedNumberOfInvocations = 2;

            ObjectManagerMock.Verify(
                m => m.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<MassUpdatePerObjectsRequest>()),
                Times.Exactly(expectedNumberOfInvocations));

            ObjectManagerMock.Verify(
                m => m.UpdateAsync(
                    It.IsAny<int>(),
                    It.Is<MassUpdatePerObjectsRequest>(
                    x => x.ObjectValues[0].Values.SequenceEqual(objectSlim.Values))),
                Times.Exactly(expectedNumberOfInvocations));
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
