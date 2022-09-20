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
        private Mock<IIAPIv2RunChecker> _iApiv2RunChecker;
        private Mock<IAPILog> _loggerMock;
        private PipelineSelector _sut;

        [SetUp]
        public void Setup()
        {
            _iApiv2RunChecker = new Mock<IIAPIv2RunChecker>();
            _iApiv2RunChecker.Setup(x => x.ShouldBeUsed()).Returns(false);

            _configurationMock = new Mock<IPipelineSelectorConfiguration>();
            _configurationMock.SetupGet(x => x.RdoArtifactTypeId)
                .Returns((int)ArtifactType.Document);

            _loggerMock = new Mock<IAPILog>();

            _sut = new PipelineSelector(_configurationMock.Object, _iApiv2RunChecker.Object, _loggerMock.Object);
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

        [Test]
        public void GetPipeline_Should_ReturnIAPI2_SyncDocumentRunPipeline_When_IIAPIv2RunChecker_ShouldBeUsed()
        {
            // Arrange
            _iApiv2RunChecker.Setup(x => x.ShouldBeUsed()).Returns(true);

            // Act
            ISyncPipeline pipeline = _sut.GetPipeline();

            // Assert
            pipeline.GetType().Should().Be<IAPI2_SyncDocumentRunPipeline>();
        }
    }
}
