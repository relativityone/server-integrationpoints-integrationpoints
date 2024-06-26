using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Unit.Pipelines
{
    [TestFixture]
    public class PipelineSelectorTests
    {
        private Mock<IPipelineSelectorConfiguration> _configurationMock;
        private Mock<IAPILog> _loggerMock;
        private PipelineSelector _sut;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IPipelineSelectorConfiguration>();
            _configurationMock.SetupGet(x => x.RdoArtifactTypeId)
                .Returns((int)ArtifactType.Document);

            _loggerMock = new Mock<IAPILog>();

            _sut = new PipelineSelector(_configurationMock.Object, _loggerMock.Object);
        }

        [Test]
        public void GetPipeline_Should_ReturnSyncDocumentRunPipeline()
        {
            // Act
            ISyncPipeline pipeline = _sut.GetPipeline();

            // Assert
            pipeline.GetType().Should().Be<SyncDocumentRunPipeline>();
        }

        [Test]
        public void GetPipeline_Should_ReturnSyncDocumentRetryPipeline_When_JobHistoryToRetryIsSet()
        {
            // Arrange
            _configurationMock.SetupGet(x => x.JobHistoryToRetryId).Returns(1);

            // Act
            ISyncPipeline pipeline = _sut.GetPipeline();

            // Assert
            pipeline.GetType().Should().Be<SyncDocumentRetryPipeline>();
        }

        [Test]
        public void GetPipeline_Should_ReturnSyncImageRetryPipeline_When_JobHistoryToRetryIsSet_And_IsImageJob()
        {
            // Arrange
            _configurationMock.SetupGet(x => x.JobHistoryToRetryId).Returns(1);
            _configurationMock.SetupGet(x => x.IsImageJob).Returns(true);

            // Act
            ISyncPipeline pipeline = _sut.GetPipeline();

            // Assert
            pipeline.GetType().Should().Be<SyncImageRetryPipeline>();
        }

        [Test]
        public void GetPipeline_Should_ReturnSyncImageRunPipeline_When_IsImageJob()
        {
            // Arrange
            _configurationMock.SetupGet(x => x.IsImageJob).Returns(true);

            // Act
            ISyncPipeline pipeline = _sut.GetPipeline();

            // Assert
            pipeline.GetType().Should().Be<SyncImageRunPipeline>();
        }
    }
}
