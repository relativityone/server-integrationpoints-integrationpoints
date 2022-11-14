using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;
using Relativity.Sync.Toggles;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.ExecutorTests
{
    internal class BatchDataSourcePreparationExecutorTests : SystemTest
    {
        private string _sourceWorkspaceName;
        private string _destinationWorkspaceName;

        private string _workspaceFileSharePath;

        private TestSyncToggleProvider _syncToggleProvider;

        [OneTimeSetUp]
        public async Task OnetimeSetUp()
        {
            _syncToggleProvider = new TestSyncToggleProvider();
            await _syncToggleProvider.SetAsync<EnableIAPIv2Toggle>(true).ConfigureAwait(false);
        }

        [SetUp]
        public void SetUp()
        {
            _sourceWorkspaceName = $"Source-{Guid.NewGuid()}";
            _destinationWorkspaceName = $"Destination-{Guid.NewGuid()}";

            _workspaceFileSharePath = Path.Combine(Path.GetTempPath(), _sourceWorkspaceName);

            Directory.CreateDirectory(_workspaceFileSharePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_workspaceFileSharePath))
            {
                Directory.Delete(_workspaceFileSharePath, true);
            }
        }

        [Test]
        public async Task ExecuteAsync_ShouldGenerateBasicLoadFile()
        {
            // Arrange
            FileShareServiceMock fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);

            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportData(dataSet: Dataset.NativesAndExtractedText, extractedText: true)
                .SetupDocumentConfiguration(
                    IdentifierFieldMap,
                    nativeFileCopyMode: ImportNativeFileCopyMode.DoNotImportNativeFiles)
                .SetupContainer(b =>
                {
                    b.RegisterInstance<IFileShareService>(fileShareMock);
                }, _syncToggleProvider)
                .PrepareBatches()
                .ExecutePreRequisteExecutor<IConfigureDocumentSynchronizationConfiguration>();

            IExecutor<IBatchDataSourcePreparationConfiguration> sut = setup.Container.Resolve<IExecutor<IBatchDataSourcePreparationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCreateSyncItemLevelErrors()
        {
            // Arrange
            FileShareServiceMock fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);
            AntiMalwareHandlerMock malwareHandlerMock = new AntiMalwareHandlerMock();
            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportData(dataSet: Dataset.NativesAndExtractedText, extractedText: true, natives: true)
                .SetupDocumentConfiguration(
                    IdentifierFieldMap)
                .SetupContainer(b =>
                {
                    b.RegisterInstance<IFileShareService>(fileShareMock);
                    b.RegisterInstance<IAntiMalwareHandler>(malwareHandlerMock);
                }, _syncToggleProvider)
                .PrepareBatches()
                .ExecutePreRequisteExecutor<IConfigureDocumentSynchronizationConfiguration>();

            string jobHistoryName = Guid.NewGuid().ToString();
            int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, setup.Configuration.SourceWorkspaceArtifactId, jobHistoryName).ConfigureAwait(false);
            setup.Configuration.JobHistoryArtifactId = jobHistoryId;

            IExecutor<IBatchDataSourcePreparationConfiguration> sut = setup.Container.Resolve<IExecutor<IBatchDataSourcePreparationConfiguration>>();

            int expectedItemLevelErrorsCount = setup.TotalDataCount;
            int createdItemLevelErrorsCount = 0;

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None);

            // Assert
            using (IObjectManager objectManager = new ServiceFactoryFromAppConfig().CreateServiceFactory().CreateProxy<IObjectManager>())
            {
                QueryRequest query = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = DefaultGuids.JobHistoryError.TypeGuid }
                };

                QueryResultSlim queryResult = await objectManager.QuerySlimAsync(setup.Configuration.SourceWorkspaceArtifactId, query, 0, int.MaxValue).ConfigureAwait(false);
                createdItemLevelErrorsCount = queryResult.Objects.Count;
            }

            createdItemLevelErrorsCount.Should().Be(expectedItemLevelErrorsCount);
        }

        [Test]
        public async Task ExecuteAsync_ShouldPauseExecution_WhenDrainStopIsRequested()
        {
            // Arrange
            FileShareServiceMock fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);
            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportData(dataSet: Dataset.NativesAndExtractedText, extractedText: true, natives: true)
                .SetupDocumentConfiguration(IdentifierFieldMap)
                .SetupContainer(b =>
                {
                    b.RegisterInstance<IFileShareService>(fileShareMock);
                })
                .PrepareBatches()
                .ExecutePreRequisteExecutor<IConfigureDocumentSynchronizationConfiguration>();

            IExecutor<IBatchDataSourcePreparationConfiguration> sut = setup.Container.Resolve<IExecutor<IBatchDataSourcePreparationConfiguration>>();

            CompositeCancellationToken token = new CompositeCancellationToken(CancellationToken.None, new CancellationToken(true), new EmptyLogger());

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, token);

            // Assert
            IBatchRepository batchRepository = setup.Container.Resolve<IBatchRepository>();
            IEnumerable<IBatch> batches = await batchRepository.GetAllAsync(
                setup.Configuration.SourceWorkspaceArtifactId,
                setup.Configuration.SyncConfigurationArtifactId,
                setup.Configuration.ExportRunId)
            .ConfigureAwait(false);

            result.Status.Should().Be(ExecutionStatus.Paused);
            batches.FirstOrDefault().Status.Should().Be(BatchStatus.Paused);
            batches.FirstOrDefault().StartingIndex.Should().Be(0);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCancelExecution_WhenCancellationIsRequested()
        {
            // Arrange
            FileShareServiceMock fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);
            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportData(dataSet: Dataset.NativesAndExtractedText, extractedText: true, natives: true)
                .SetupDocumentConfiguration(IdentifierFieldMap)
                .SetupContainer(b =>
                {
                    b.RegisterInstance<IFileShareService>(fileShareMock);
                })
                .PrepareBatches()
                .ExecutePreRequisteExecutor<IConfigureDocumentSynchronizationConfiguration>();

            IExecutor<IBatchDataSourcePreparationConfiguration> sut = setup.Container.Resolve<IExecutor<IBatchDataSourcePreparationConfiguration>>();
            CompositeCancellationToken token = new CompositeCancellationToken(new CancellationToken(true), CancellationToken.None, new EmptyLogger());

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, token);

            // Assert
            IBatchRepository batchRepository = setup.Container.Resolve<IBatchRepository>();
            IEnumerable<IBatch> batches = await batchRepository.GetAllAsync(
                setup.Configuration.SourceWorkspaceArtifactId,
                setup.Configuration.SyncConfigurationArtifactId,
                setup.Configuration.ExportRunId)
            .ConfigureAwait(false);

            result.Status.Should().Be(ExecutionStatus.Completed);
            batches.FirstOrDefault().Status.Should().Be(BatchStatus.Cancelled);
        }
    }
}
