
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class RetryDataSourceSnapshotExecutorTests
    {
        private DataSourceSnapshotExecutor _instance;

        private Mock<IObjectManager> _objectManager;
        private Mock<IRetryDataSourceSnapshotConfiguration> _configuration;
        private Mock<IJobProgressUpdater> _jobProgressUpdater;
        private Mock<ISnapshotQueryRequestProvider> _snapshotQueryProviderFake;

        private const int _WORKSPACE_ID = 458712;
        private const int _DATA_SOURCE_ID = 485219;
        private const int _JOB_HISOTRY_TO_RETRY_ID = 987654;

        [SetUp]
        public void SetUp()
        {
            _objectManager = new Mock<IObjectManager>();

            Mock<ISourceServiceFactoryForUser> serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            _configuration = new Mock<IRetryDataSourceSnapshotConfiguration>();
            _configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            _configuration.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);
            _configuration.Setup(x => x.JobHistoryToRetryId).Returns(_JOB_HISOTRY_TO_RETRY_ID);
            _configuration.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

            _snapshotQueryProviderFake = new Mock<ISnapshotQueryRequestProvider>();

            _jobProgressUpdater = new Mock<IJobProgressUpdater>();
            Mock<IJobProgressUpdaterFactory> jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
            jobProgressUpdaterFactory.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdater.Object);

            _instance = new DataSourceSnapshotExecutor(serviceFactoryForUser.Object, jobProgressUpdaterFactory.Object,
                new EmptyLogger(), _snapshotQueryProviderFake.Object);
        }

        [Test]
        public async Task ItShouldInitializeExportAndSaveResult()
        {
            const int totalRecords = 123456789;
            Guid runId = Guid.NewGuid();

            ExportInitializationResults exportInitializationResults = new ExportInitializationResults
            {
                RecordCount = totalRecords,
                RunID = runId
            };
            _objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);
            _objectManager.Setup(x => x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(Array.Empty<RelativityObjectSlim>());

            // ACT
            ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Completed);
            _configuration.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
            _jobProgressUpdater.Verify(x => x.SetTotalItemsCountAsync(totalRecords));
        }

        [Test]
        public async Task ItShouldFailWhenExportApiFails()
        {
            _objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

            // ACT
            ExecutionResult executionResult = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            executionResult.Status.Should().Be(ExecutionStatus.Failed);
            executionResult.Exception.Should().BeOfType<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldIncludeFieldsFromFieldMapping()
        {
            ExportInitializationResults exportInitializationResults = new ExportInitializationResults
            {
                RecordCount = 1L,
                RunID = Guid.NewGuid()
            };
            _objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

            // ACT
            await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            _objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1));
        }

        [TestCase(ImportOverwriteMode.AppendOverlay)]
        [TestCase(ImportOverwriteMode.AppendOnly)]
        [TestCase(ImportOverwriteMode.OverlayOnly)]
        public async Task ItShouldNotChangeOverrideModeFromConfig(ImportOverwriteMode previousOverrideMode)
        {
            // Arrange
            _configuration.SetupProperty(x => x.ImportOverwriteMode, previousOverrideMode);

            // Act
            await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _configuration.Object.ImportOverwriteMode.Should().Be(previousOverrideMode);
        }
    }
}
