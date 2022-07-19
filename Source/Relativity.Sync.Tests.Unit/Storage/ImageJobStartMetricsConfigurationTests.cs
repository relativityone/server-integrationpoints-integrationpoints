using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal class ImageJobStartMetricsConfigurationTests : ConfigurationTestBase
    {
        private JSONSerializer _serializer;

        private ImageJobStartMetricsConfiguration _sut;

        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
        private const int _USER_ID = 323454;
        private readonly Guid _WORKFLOW_ID = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _serializer = new JSONSerializer();

            _sut = new ImageJobStartMetricsConfiguration(_configuration, _serializer, new SyncJobParameters(1, _SOURCE_WORKSPACE_ARTIFACT_ID, _USER_ID, _WORKFLOW_ID));
        }

        [Test]
        public void SourceWorkspaceArtifactId_ShouldReturnProperValue()
        {
            // Act & Assert
            _sut.SourceWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
        }

        [Test]
        public void JobHistoryToRetryId_ShouldReturnProperValue()
        {
            // Arrange
            const int jobHistoryArtifactId = 104799;
            _configurationRdo.JobHistoryToRetryId = jobHistoryArtifactId;

            // Act & Assert
            _sut.JobHistoryToRetryId.Should().Be(jobHistoryArtifactId);
        }

        [Test]
        public void DestinationWorkspaceArtifactId_ShouldReturnProperValue()
        {
            // Arrange
            const int destinationWorkspaceArtifactId = 106799;
            _configurationRdo.DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId;

            // Act & Assert
            _sut.DestinationWorkspaceArtifactId.Should().Be(destinationWorkspaceArtifactId);
        }

        [Test]
        public void ProductionImagePrecedence_ShouldReturnProperValue()
        {
            // Arrange
            var expectedValue = new[] { 1, 2, 3 };
            _configurationRdo.ProductionImagePrecedence = _serializer.Serialize(expectedValue);

            // Act & Assert
            _sut.ProductionImagePrecedence.Should().BeEquivalentTo(expectedValue);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IncludeOriginalImageIfNotFoundInProductions_ShouldBeRetrieved(bool expectedValue)
        {
            // Arrange
            _configurationRdo.IncludeOriginalImages = expectedValue;

            // Act & Assert
            _sut.IncludeOriginalImageIfNotFoundInProductions.Should().Be(expectedValue);
        }
    }
}
