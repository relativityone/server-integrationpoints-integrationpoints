using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class DocumentSynchronizationMonitorExecutorTests
    {
        private const int _DESTINATION_WORKSPACE_ID = -1000001;
        private const string _EXPORT_RUN_ID = "11111111-2222-3333-4444-555555555555";

        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IImportSourceController> _sourceControllerMock;
        private Mock<IImportJobController> _jobControllerMock;
        private Mock<IDocumentSynchronizationMonitorConfiguration> _configurationMock;

        private DocumentSynchronizationMonitorExecutor _sut;

        [SetUp]
        public void Setup()
        {
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            _loggerMock = new Mock<IAPILog>();
            _sourceControllerMock = new Mock<IImportSourceController>();
            _jobControllerMock = new Mock<IImportJobController>();
            _configurationMock = new Mock<IDocumentSynchronizationMonitorConfiguration>();

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportSourceController>()).ReturnsAsync(_sourceControllerMock.Object);
            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportJobController>()).ReturnsAsync(_jobControllerMock.Object);

            _configurationMock.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ID);
            _configurationMock.Setup(x => x.ExportRunId).Returns(new Guid(_EXPORT_RUN_ID));

            _sut = new DocumentSynchronizationMonitorExecutor(_serviceFactoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallForJobStatus_UntilJobIsFinished()
        {
            // Arrange
            int expectedNumberOfJobStatusCalls = 3;
            PrepareTestDataSources();

            ImportDetails firstIterationDetailsResponse = PrepareTestImportDetailsContent(ImportState.Scheduled);
            ImportDetails secondIterationDetailsResponse = PrepareTestImportDetailsContent(ImportState.Inserting);
            ImportDetails thirdIterationDetailsResponse = PrepareTestImportDetailsContent(ImportState.Completed);

            _jobControllerMock.SetupSequence(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    firstIterationDetailsResponse))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    secondIterationDetailsResponse))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    thirdIterationDetailsResponse));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Exactly(expectedNumberOfJobStatusCalls));
        }

        [TestCase(ImportState.Canceled, "Canceled")]
        [TestCase(ImportState.Failed, "Failed")]
        [TestCase(ImportState.Completed, "Completed")]
        public async Task ExecuteAsync_ShouldReturnCorrectStatus_WhenImportStateIsKnown(ImportState jobFinalState, string expectedStatus)
        {
            // Arrange
            PrepareTestDataSources();

            ImportDetails jobDetailsResponse = PrepareTestImportDetailsContent(jobFinalState);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    jobDetailsResponse));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.ToString().Should().Be(expectedStatus);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnCompletedWithErrorsStatus_WhenAtLeastOneDataSourceHasItemLevelErrors()
        {
            // Arrange
            PrepareTestDataSources(DataSourceState.CompletedWithItemErrors);

            ImportDetails jobDetailsResponse = PrepareTestImportDetailsContent(ImportState.Completed);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    jobDetailsResponse));

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.CompletedWithErrors);
            _jobControllerMock.Verify(
                x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailedStatus_WhenMonitoringErrorOccurrs()
        {
            // Arrange
            string testErrorMessage = "test error message";
            string testErrorCode = "ABC";
            PrepareTestDataSources(DataSourceState.Inserting);

            ImportDetails jobDetailsResponse = PrepareTestImportDetailsContent(ImportState.Completed);

            _jobControllerMock.Setup(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId))
                .ReturnsAsync(new ValueResponse<ImportDetails>(
                    It.IsAny<Guid>(),
                    false,
                    testErrorMessage,
                    testErrorCode,
                    null));
            string expectedErrorMessage = $"Job progress monitoring failed. Error code: {testErrorCode}, message: {testErrorMessage}";

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configurationMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Be(expectedErrorMessage);
            _loggerMock.Verify(x => x.LogError(It.IsAny<SyncException>(), "Document synchronization monitoring error"));
        }

        private void PrepareTestDataSources(DataSourceState testedSourceState = DataSourceState.Completed)
        {
            List<Guid> sourceGuids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            DataSources sources = new DataSources(sourceGuids, 2);

            ValueResponse<DataSources> response = new ValueResponse<DataSources>(new Guid(_EXPORT_RUN_ID), true, string.Empty, string.Empty, sources);

            _jobControllerMock.Setup(x => x.GetSourcesAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId)).ReturnsAsync(response);
            SetupDataSourceResultsSequence(testedSourceState);
        }

        private void SetupDataSourceResultsSequence(DataSourceState testedSourceState)
        {
            _sourceControllerMock.SetupSequence(x => x.GetDetailsAsync(_configurationMock.Object.DestinationWorkspaceArtifactId, _configurationMock.Object.ExportRunId, It.IsAny<Guid>()))
                .ReturnsAsync(new ValueResponse<DataSourceDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DataSourceDetails() { State = testedSourceState }))
                .ReturnsAsync(new ValueResponse<DataSourceDetails>(
                    It.IsAny<Guid>(),
                    true,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DataSourceDetails() { State = DataSourceState.Completed }));
        }

        private ImportDetails PrepareTestImportDetailsContent(ImportState state)
        {
            return new ImportDetails(state, "testApp", 0, DateTime.Now, 0, DateTime.Now);
        }
    }
}
