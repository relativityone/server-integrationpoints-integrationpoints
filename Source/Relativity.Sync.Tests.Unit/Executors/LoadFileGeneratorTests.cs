using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class LoadFileGeneratorTests
    {
        private const string _BATCH_GUID = "00000000-2222-4444-0000-888888888888";
        private const string _EXPORT_RUN_ID = "11111111-6666-2222-7777-333333333333";
        private const int _DESTINATION_WORKSPACE_ID = -1000001;
        private const int _SOURCE_WORKSPACE_ID = -1000000;
        private const int _CONFIGURATION_ID = -12678;

        private Mock<IBatchDataSourcePreparationConfiguration> _configurationMock;
        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
        private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactoryMock;
        private Mock<ISourceWorkspaceDataReader> _dataReaderMock;
        private Mock<IWorkspaceManager> _workspaceManagerMock;
        private Mock<IBatch> _batchMock;
        private Mock<IAPILog> _loggerMock;

        private string _serverPath;
        private string _workspacePath;

        private LoadFileGenerator _sut;

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<IBatchDataSourcePreparationConfiguration>();
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            _dataReaderFactoryMock = new Mock<ISourceWorkspaceDataReaderFactory>();
            _dataReaderMock = new Mock<ISourceWorkspaceDataReader>();
            _workspaceManagerMock = new Mock<IWorkspaceManager>();
            _batchMock = new Mock<IBatch>();
            _loggerMock = new Mock<IAPILog>();

            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(It.IsAny<Guid>());
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);
            _configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_CONFIGURATION_ID);

            _batchMock.Setup(x => x.BatchGuid).Returns(new Guid(_BATCH_GUID));
            _batchMock.Setup(x => x.ExportRunId).Returns(new Guid(_EXPORT_RUN_ID));

            _dataReaderFactoryMock.Setup(x => x.CreateNativeSourceWorkspaceDataReader(_batchMock.Object, CancellationToken.None)).Returns(_dataReaderMock.Object);

            _sut = new LoadFileGenerator(_configurationMock.Object, _serviceFactoryMock.Object, _dataReaderFactoryMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_workspacePath) && Directory.Exists(_workspacePath))
            {
                var dir = new DirectoryInfo(_workspacePath);
                dir.Delete(recursive: true);
            }

            _serverPath = null;
            _workspacePath = null;
        }

        [Test]
        public async Task Generate_ShouldReturnILoadFileWithCorrectId()
        {
            // Arrange
            PrepareFakeLoadFilePath();
            FileShareResourceServer server = new FileShareResourceServer { UNCPath = _serverPath };

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);
            _workspaceManagerMock.Setup(x => x.GetDefaultWorkspaceFileShareResourceServerAsync(It.Is<WorkspaceRef>(y => y.ArtifactID == _DESTINATION_WORKSPACE_ID)))
                .ReturnsAsync(server);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Id.ToString().Should().Be(_BATCH_GUID);
        }

        [Test]
        public async Task Generate_ShouldReturnCorrectFilePath_WhenRootDirectoryExists()
        {
            // Arrange
            string expectedBatchPath = PrepareFakeLoadFilePath();
            FileShareResourceServer server = new FileShareResourceServer { UNCPath = _serverPath };

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);
            _workspaceManagerMock.Setup(x => x.GetDefaultWorkspaceFileShareResourceServerAsync(It.Is<WorkspaceRef>(y => y.ArtifactID == _DESTINATION_WORKSPACE_ID)))
                .ReturnsAsync(server);

            // Act
            ILoadFile result = await _sut.GenerateAsync(_batchMock.Object).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Path.Should().Be(expectedBatchPath);
            result.Settings.Path.Should().Be(expectedBatchPath);
        }

        [Test]
        public void Generate_ShouldReturnError_WhenRootDirectoryDoesNotExist()
        {
            // Arrange
            _serverPath = "randomTestPath";
            string rootDirectory = $@"{_serverPath}\EDDS{_DESTINATION_WORKSPACE_ID}";
            string expectedErrorMessage = $"Unable to create load file path. Directory: {rootDirectory} does not exist!";
            FileShareResourceServer server = new FileShareResourceServer { UNCPath = _serverPath };

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);
            _workspaceManagerMock.Setup(x => x.GetDefaultWorkspaceFileShareResourceServerAsync(It.Is<WorkspaceRef>(y => y.ArtifactID == _DESTINATION_WORKSPACE_ID)))
                .ReturnsAsync(server);

            // Act
            Func<Task<ILoadFile>> function = async () => await _sut.GenerateAsync(_batchMock.Object).ConfigureAwait(false);

            // Assert
            function.Should().Throw<Exception>().WithMessage(expectedErrorMessage);
        }

        private string PrepareFakeLoadFilePath()
        {
            _serverPath = Path.GetTempPath();
            _workspacePath = Path.Combine(_serverPath, $@"EDDS{_DESTINATION_WORKSPACE_ID}");

            if (!Directory.Exists(_workspacePath))
            {
                Directory.CreateDirectory(_workspacePath);
            }

            return $@"{_workspacePath}\Sync\{_EXPORT_RUN_ID}\{_BATCH_GUID}\{_BATCH_GUID}.dat";
        }
    }
}
