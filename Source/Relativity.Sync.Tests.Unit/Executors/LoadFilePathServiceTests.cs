using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal class LoadFilePathServiceTests
    {
        private const int _DESTINATION_WORKSPACE_ID = 100;

        private Mock<IFileShareService> _fileShareServiceMock;

        private LoadFilePathService _sut;

        private Guid _exportRunId;

        private string WorkspaceFileSharePath => Path.GetTempPath();
        private string SyncJobPath => Path.Combine(WorkspaceFileSharePath, "Sync", _exportRunId.ToString());

        [SetUp]
        public void SetUp()
        {
            _exportRunId = Guid.NewGuid();

            _fileShareServiceMock = new Mock<IFileShareService>();
            _fileShareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(WorkspaceFileSharePath);

            Mock<ILoadFileConfiguration> configuration = new Mock<ILoadFileConfiguration>();
            configuration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            configuration.SetupGet(x => x.ExportRunId).Returns(_exportRunId);

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new LoadFilePathService(
                _fileShareServiceMock.Object,
                configuration.Object,
                new MemoryCacheWrapper(),
                log.Object);
        }

        [Test]
        public async Task GetJobDirectoryPathAsync_ShouldReturnPath()
        {
            // Arrange
            string expectedPath = SyncJobPath;

            // Act
            string path = await _sut.GetJobDirectoryPathAsync().ConfigureAwait(false);

            // Assert
            path.Should().Be(expectedPath);
        }

        [Test]
        public void GetJobDirectoryPathAsync_ShouldThrow_WhenWorkspaceFileshareDoesNotExist()
        {
            // Arrange
            string nonExistingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            _fileShareServiceMock.Setup(x => x.GetWorkspaceFileShareLocationAsync(_DESTINATION_WORKSPACE_ID))
                .ReturnsAsync(nonExistingDirectory);

            // Act
            Func<Task<string>> func = async () => await _sut.GetJobDirectoryPathAsync().ConfigureAwait(false);

            // Assert
            func.Should().Throw<DirectoryNotFoundException>();
        }

        [Test]
        public async Task GetJobDirectoryPathAsync_ShouldReturnPathFromCache_WhenMethodRunSecondTime()
        {
            // Arrange
            string expectedPath = SyncJobPath;

            await _sut.GetJobDirectoryPathAsync().ConfigureAwait(false);

            // Act
            string path = await _sut.GetJobDirectoryPathAsync().ConfigureAwait(false);

            // Assert
            path.Should().Be(expectedPath);

            _fileShareServiceMock.Verify(
                x => x.GetWorkspaceFileShareLocationAsync(
                        _DESTINATION_WORKSPACE_ID),
                Times.Once);
        }

        [Test]
        public async Task GenerateBatchLoadFilePathAsync_ShouldReturnBatchLoadFilePath()
        {
            // Arrange
            BatchStub batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
            };

            string expectedPath = Path.Combine(SyncJobPath, $"{batch.BatchGuid}.dat");

            // Act
            string path = await _sut.GenerateBatchLoadFilePathAsync(batch).ConfigureAwait(false);

            // Assert
            path.Should().Be(expectedPath);
        }

        [Test]
        public async Task GenerateLongTextFilePathAsync_ShouldReturnLongTextFilePath()
        {
            // Arrange
            Guid longTextId = new Guid("D66BDE29-D2EA-4C82-8592-DD24501FF985");

            string expectedPath = Path.Combine(SyncJobPath, "LongTexts", "d6", $"{longTextId}.txt");

            // Act
            string path = await _sut.GenerateLongTextFilePathAsync(longTextId).ConfigureAwait(false);

            // Assert
            path.Should().Be(expectedPath);
        }

        [Test]
        public async Task GetLoadFileRelativeLongTextFilePathAsync()
        {
            // Arrange
            string expectedRelativePath = "LongTexts\\EB\\EB9EE822-2154-4A28-A3A5-79A3A0BF7310";

            string longTextPath = Path.Combine(SyncJobPath, expectedRelativePath);

            // Act
            string path = await _sut.GetLoadFileRelativeLongTextFilePathAsync(longTextPath).ConfigureAwait(false);

            // Assert
            path.Should().Be(expectedRelativePath);
        }
    }
}
