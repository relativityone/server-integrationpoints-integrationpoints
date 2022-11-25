using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

using Relativity.Sync.Tests.Common.Stubs;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class ImportApiItemLevelErrorHandlerTests
    {
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 1001;
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1002;
        private const int _JOB_HISTORY_ARTIFACT_ID = 1003;
        private const string _ERROR_CODE = "Error Code";
        private const string _ERROR_MESSAGE = "Error Message";
        private const string _IDENTIFIER = "identifier";

        private Guid _jobId = Guid.NewGuid();

        private Mock<IItemLevelErrorHandlerConfiguration> _configurationMock;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepositoryFake;
        private Mock<IDocumentSynchronizationMonitorConfiguration> _documentSyncMonitorConfigurationMock;
        private Mock<IImportSourceController> _importSourceControllerMock;

        private ImportApiItemLevelErrorHandler _sut;

        [SetUp]
        public void Setup()
        {
            PrepareFakeConfiguration();
            _jobHistoryErrorRepositoryFake = new Mock<IJobHistoryErrorRepository>();
            _documentSyncMonitorConfigurationMock = PrepareDocumentSyncMonitorConfigurationMock();
            _importSourceControllerMock = new Mock<IImportSourceController>();

            _sut = new ImportApiItemLevelErrorHandler(_configurationMock.Object, _jobHistoryErrorRepositoryFake.Object);
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldCreateItemLevelErrorsInJobHistory_WhenTheyExists()
        {
            // Arrange
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            IBatch batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
                Status = BatchStatus.CompletedWithErrors
            };

            PrepareGetItemErrorsAsync(importSourceControllerMock, _IDENTIFIER, batch);

            // Act
            await _sut.HandleItemLevelErrorsAsync(importSourceControllerMock.Object, batch, _documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        _SOURCE_WORKSPACE_ARTIFACT_ID,
                        _JOB_HISTORY_ARTIFACT_ID,
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Once);
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldNotCreateItemLevelErrorsInJobHistory_WhenTheyDontExists()
        {
            // Arrange
            IBatch batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
                Status = BatchStatus.CompletedWithErrors
            };

            PrepareGetItemErrorsAsync(_importSourceControllerMock, _IDENTIFIER, batch, itemLevelErrorsPerBatch: 0);

            // Act
            await _sut.HandleItemLevelErrorsAsync(_importSourceControllerMock.Object, batch, _documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        _SOURCE_WORKSPACE_ARTIFACT_ID,
                        _JOB_HISTORY_ARTIFACT_ID,
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldNotCreateItemLevelErrorsInJobHistory_WhenBatchStatusIsCompleted()
        {
            // Arrange
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            IBatch batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
                Status = BatchStatus.Completed
            };

            PrepareGetItemErrorsAsync(importSourceControllerMock, _IDENTIFIER, batch);

            // Act
            await _sut.HandleItemLevelErrorsAsync(importSourceControllerMock.Object, batch, _documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        _SOURCE_WORKSPACE_ARTIFACT_ID,
                        _JOB_HISTORY_ARTIFACT_ID,
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public void HandleIApiItemLevelErrors_ShouldThrowException_WhenGetItemErrorsAsyncCallFails()
        {
            // Arrange
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            IBatch batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
                Status = BatchStatus.CompletedWithErrors
            };

            PrepareGetItemErrorsAsync(importSourceControllerMock, _IDENTIFIER, batch, false);

            // Act
            Func<Task> function = async () => await _sut.HandleItemLevelErrorsAsync(importSourceControllerMock.Object, batch, _documentSyncMonitorConfigurationMock.Object);

            // Assert
            function.Should().Throw<Exception>();
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        _SOURCE_WORKSPACE_ARTIFACT_ID,
                        _JOB_HISTORY_ARTIFACT_ID,
                        It.IsAny<List<CreateJobHistoryErrorDto>>()),
                Times.Never);
        }

        [Test]
        public async Task HandleIApiItemLevelErrors_ShouldCreateItemLevelErrorsInJobHistory_WhenThereIsNoIdentifierInResponse()
        {
            // Arrange
            Mock<IImportSourceController> importSourceControllerMock = new Mock<IImportSourceController>();
            IBatch batch = new BatchStub
            {
                BatchGuid = Guid.NewGuid(),
                Status = BatchStatus.CompletedWithErrors
            };

            PrepareGetItemErrorsAsync(importSourceControllerMock, _IDENTIFIER, batch, identifierName: string.Empty);

            // Act
            await _sut.HandleItemLevelErrorsAsync(importSourceControllerMock.Object, batch, _documentSyncMonitorConfigurationMock.Object);

            // Assert
            _jobHistoryErrorRepositoryFake.Verify(
                x =>
                    x.MassCreateAsync(
                        _SOURCE_WORKSPACE_ARTIFACT_ID,
                        _JOB_HISTORY_ARTIFACT_ID,
                        It.Is<List<CreateJobHistoryErrorDto>>(
                            xx =>
                            xx.Select(xxx => xxx.ErrorMessage)
                                .All(xxxx => xxxx == $"It was impossible to determine document identifier. ErrorMessage: {_ERROR_MESSAGE}"))),
                Times.Once);
        }

        private void PrepareFakeConfiguration()
        {
            _configurationMock = new Mock<IItemLevelErrorHandlerConfiguration>();
            _configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
            _configurationMock.Setup(x => x.JobHistoryArtifactId).Returns(_JOB_HISTORY_ARTIFACT_ID);
        }

        private void PrepareGetItemErrorsAsync(
            Mock<IImportSourceController> importSourceControllerMock,
            string identifier,
            IBatch batch,
            bool isSuccessful = true,
            string identifierName = "Identifier",
            int itemLevelErrorsPerBatch = 1000)
        {
            List<ImportError> itemLevelErrorsList = new List<ImportError>();
            for (int i = 0; i < itemLevelErrorsPerBatch; i++)
            {
                ImportError importError = new ImportError(0, new List<ErrorDetail>
                {
                    new ErrorDetail(
                        0,
                        _ERROR_CODE,
                        _ERROR_MESSAGE,
                        string.Empty,
                        new Dictionary<string, string>
                        {
                            { identifierName, identifier }
                        })
                });
                itemLevelErrorsList.Add(importError);
            }

            ValueResponse<ImportErrors> itemLevelErrorsResponse = new ValueResponse<ImportErrors>(
                _jobId,
                isSuccessful,
                string.Empty,
                string.Empty,
                new ImportErrors(
                    Guid.Empty,
                    itemLevelErrorsList,
                    itemLevelErrorsList.Count,
                    0,
                    itemLevelErrorsList.Count));

            importSourceControllerMock.Setup(x =>
                    x.GetItemErrorsAsync(
                        _DESTINATION_WORKSPACE_ARTIFACT_ID,
                        _jobId,
                        It.Is<Guid>(y => y == batch.BatchGuid),
                        0,
                        int.MaxValue))
                .ReturnsAsync(itemLevelErrorsResponse);
        }

        private Mock<IDocumentSynchronizationMonitorConfiguration> PrepareDocumentSyncMonitorConfigurationMock()
        {
            Mock<IDocumentSynchronizationMonitorConfiguration> documentSyncMonitorConfigurationMock =
                new Mock<IDocumentSynchronizationMonitorConfiguration>();

            documentSyncMonitorConfigurationMock
                .Setup(x => x.DestinationWorkspaceArtifactId)
                .Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);
            documentSyncMonitorConfigurationMock
                .Setup(x => x.ExportRunId)
                .Returns(_jobId);
            return documentSyncMonitorConfigurationMock;
        }
    }
}
