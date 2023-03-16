using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models.Settings;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal class ConfigureDocumentSynchronizationExecutorTests
    {
        private Mock<IConfigureDocumentSynchronizationConfiguration> _executorConfigurationMock;
        private Mock<IImportSettingsBuilder> _settingsBuilderFake;
        private Mock<IImportService> _importServiceMock;

        private ConfigureDocumentSynchronizationExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _executorConfigurationMock = new Mock<IConfigureDocumentSynchronizationConfiguration>();

            _settingsBuilderFake = new Mock<IImportSettingsBuilder>();
            _settingsBuilderFake.Setup(x => x.BuildAsync(It.IsAny<IConfigureDocumentSynchronizationConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ImportSettings(new ImportDocumentSettings(), new AdvancedImportSettings()));

            _importServiceMock = new Mock<IImportService>();

            _sut = new ConfigureDocumentSynchronizationExecutor(
                FakeHelper.CreateSyncJobParameters(),
                _settingsBuilderFake.Object,
                _importServiceMock.Object,
                Mock.Of<IAPILog>());
        }

        [Test]
        public async Task ExecuteAsync_ShouldCreateAndConfigureAndBeginJob()
        {
            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            _importServiceMock.Verify(
                x => x.CreateImportJobAsync(It.IsAny<SyncJobParameters>()), Times.Once);
            _importServiceMock.Verify(
                x => x.ConfigureDocumentImportSettingsAsync(It.IsAny<ImportSettings>()), Times.Once);
            _importServiceMock.Verify(
                x => x.BeginImportJobAsync(), Times.Once);

            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_WhenJobCreationFails_ShouldFail()
        {
            _importServiceMock.Setup(x => x.CreateImportJobAsync(It.IsAny<SyncJobParameters>())).Throws(new SyncException());

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importServiceMock.Verify(
                x => x.CreateImportJobAsync(It.IsAny<SyncJobParameters>()), Times.Once);
            _importServiceMock.Verify(
                x => x.ConfigureDocumentImportSettingsAsync(It.IsAny<ImportSettings>()), Times.Never);
            _importServiceMock.Verify(
                x => x.BeginImportJobAsync(), Times.Never);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_WhenDocumentConfigurationFails_ShouldFail()
        {
            _importServiceMock.Setup(
                x => x.ConfigureDocumentImportSettingsAsync(It.IsAny<ImportSettings>())).Throws(new SyncException());

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importServiceMock.Verify(
                x => x.CreateImportJobAsync(It.IsAny<SyncJobParameters>()), Times.Once);
            _importServiceMock.Verify(
                 x => x.ConfigureDocumentImportSettingsAsync(It.IsAny<ImportSettings>()), Times.Once);
            _importServiceMock.Verify(
                x => x.BeginImportJobAsync(), Times.Never);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_WhenJobBeginFails_ShouldFail()
        {
            _importServiceMock.Setup(x =>
                    x.BeginImportJobAsync()).Throws(new SyncException());

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importServiceMock.Verify(
                x => x.CreateImportJobAsync(It.IsAny<SyncJobParameters>()), Times.Once);
            _importServiceMock.Verify(
                x => x.ConfigureDocumentImportSettingsAsync(It.IsAny<ImportSettings>()), Times.Once);
            _importServiceMock.Verify(
                x => x.BeginImportJobAsync(), Times.Once);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_InCaseOfException_ShouldFail()
        {
            _settingsBuilderFake.Setup(x => x.BuildAsync(It.IsAny<IConfigureDocumentSynchronizationConfiguration>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }
    }
}
