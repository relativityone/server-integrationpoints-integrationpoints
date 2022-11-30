using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    internal class JobStatusConsolidationExecutorTests : SystemTest
    {
        private WorkspaceRef _sourceWorkspace;
        private WorkspaceRef _destinationWorkspace;

        private IBatchRepository _batchRepository;

        private static readonly Guid JobHistoryGuid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");
        private static readonly Guid CompletedItemsCountGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
        private static readonly Guid FailedItemsCountGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
        private static readonly Guid TotalItemsCountGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");
        private static readonly Guid ExportRunId = new Guid("81F9F85A-E568-4F2B-B34F-04992FC5EA54");

        protected override Task ChildSuiteSetup()
        {
            _batchRepository = new BatchRepository(new TestRdoManager(Logger), new ServiceFactoryStub(ServiceFactory), new DateTimeWrapper());
            return Task.CompletedTask;
        }

        [SetUp]
        public async Task SetUp()
        {
            Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
            Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();

            WorkspaceRef[] createdWorkspaces = await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
            _sourceWorkspace = createdWorkspaces[0];
            _destinationWorkspace = createdWorkspaces[1];
        }

        [IdentifiedTest("412f4e45-e76e-4aaf-abec-25d665a9e57d")]
        public async Task SyncJob_ShouldConsolidateJobStatusAndUpdateJobHistory()
        {
            // Arrange
            int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            var configuration = new ConfigurationStub
            {
                DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
                SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
                JobHistoryArtifactId = jobHistoryArtifactId,
            };

            configuration.SetJobStatusConsolidationConfigurationExportRunId(ExportRunId);

            configuration.SyncConfigurationArtifactId = await Rdos
                .CreateSyncConfigurationRdoAsync(_sourceWorkspace.ArtifactID, configuration).ConfigureAwait(false);

            const int batchCount = 3;
            const int transferredItemsCountPerBatch = 10000;
            const int failedItemsCountPerBatch = 500;
            await CreateBatchesAsync(_sourceWorkspace.ArtifactID, configuration.SyncConfigurationArtifactId, batchCount, transferredItemsCountPerBatch, failedItemsCountPerBatch).ConfigureAwait(false);

            ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IJobStatusConsolidationConfiguration>(configuration);

            // Act
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            const int expectedTransferredItemsCount = transferredItemsCountPerBatch * batchCount;
            const int expectedFailedItemsCount = failedItemsCountPerBatch * batchCount;
            const int expectedTotalItemsCount = expectedTransferredItemsCount + expectedFailedItemsCount;

            var (actualTransferredItemsCount, actualFailedItemsCount, actualTotalItemsCount) = await ReadJobHistoryAsync(_sourceWorkspace.ArtifactID, jobHistoryArtifactId).ConfigureAwait(false);

            actualTransferredItemsCount.Should().Be(expectedTransferredItemsCount);
            actualFailedItemsCount.Should().Be(expectedFailedItemsCount);
            actualTotalItemsCount.Should().Be(expectedTotalItemsCount);
        }

        private async Task<(int transferredItemsCount, int failedItemsCount, int totalItemsCount)> ReadJobHistoryAsync(int workspaceArtifactId, int jobHistoryArtifactId)
        {
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        Guid = JobHistoryGuid
                    },
                    Condition = $"'ArtifactID' == {jobHistoryArtifactId}",
                    Fields = new[]
                    {
                        new FieldRef
                        {
                            Guid = TotalItemsCountGuid
                        },
                        new FieldRef
                        {
                            Guid = CompletedItemsCountGuid
                        },
                        new FieldRef
                        {
                            Guid = FailedItemsCountGuid
                        }
                    }
                };

                QueryResult result = await objectManager.QueryAsync(workspaceArtifactId, request, start: 0, length: 1).ConfigureAwait(false);

                if (!result.Objects.Any())
                {
                    throw new SyncException($"Query for job history artifact ID: {jobHistoryArtifactId} returned empty results.");
                }
                else
                {
                    RelativityObject jobHistory = result.Objects.First();
                    int completedItemsCount = (int)jobHistory[CompletedItemsCountGuid].Value;
                    int failedItemsCount = (int)jobHistory[FailedItemsCountGuid].Value;
                    int totalItemsCount = (int)jobHistory[TotalItemsCountGuid].Value;

                    return (completedItemsCount, failedItemsCount, totalItemsCount);
                }
            }
        }

        private async Task CreateBatchesAsync(
            int workspaceArtifactId,
            int syncConfigurationArtifactId,
            int batchCount,
            int transferredItemsCountPerBatch,
            int failedItemsCountPerBatch)
        {
            int itemsCountPerBatch = transferredItemsCountPerBatch + failedItemsCountPerBatch;

            IEnumerable<Task<IBatch>> batchCreationTasks = Enumerable
                .Range(0, batchCount)
                .Select(i => _batchRepository
                    .CreateAsync(
                        workspaceArtifactId,
                        syncConfigurationArtifactId,
                        ExportRunId,
                        itemsCountPerBatch,
                        i * itemsCountPerBatch));

            IBatch[] batches = await Task.WhenAll(batchCreationTasks).ConfigureAwait(false);

            foreach (IBatch batch in batches)
            {
                await batch.SetTransferredItemsCountAsync(transferredItemsCountPerBatch).ConfigureAwait(false);
                await batch.SetFailedItemsCountAsync(failedItemsCountPerBatch).ConfigureAwait(false);
            }
        }
    }
}
