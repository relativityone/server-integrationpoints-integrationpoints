using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal sealed class ObjectLinkingSnapshotPartitionExecutorTests : SystemTest
    {
        private int _workspaceId;
        private int _syncConfigurationId;
        private ObjectLinkingSnapshotPartitionExecutor _instance;
        private IBatchRepository _batchRepository;
        
        private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
        private static readonly Guid TotalDocumentsCountGuid = new Guid("C30CE15E-45D6-49E6-8F62-7C5AA45A4694");
        private static readonly Guid _SYNC_BATCH_OBJECT_TYPE = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private static readonly Guid _EXPORT_RUN_ID = new Guid("2574E4D4-F067-4D1C-A534-87C9C140FB20");

        private static ObjectTypeRef SyncBatchObjectTypeRef => new ObjectTypeRef { Guid = _SYNC_BATCH_OBJECT_TYPE };

        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync()
                .ConfigureAwait(false);
            _workspaceId = workspace.ArtifactID;

            int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspaceId).ConfigureAwait(false);
            _syncConfigurationId = await Rdos.CreateSyncConfigurationRdoAsync(_workspaceId, jobHistoryId).ConfigureAwait(false);

            _batchRepository = new BatchRepository(new TestRdoManager(Logger), new ServiceFactoryStub(ServiceFactory), new DateTimeWrapper());
            _instance = new ObjectLinkingSnapshotPartitionExecutor(_batchRepository, new EmptyLogger());
        }

        [SetUp]
        public async Task SetUp()
        {
            // Clean up all existing batches
            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                var request = new MassDeleteByCriteriaRequest
                {
                    ObjectIdentificationCriteria = new ObjectIdentificationCriteria
                    {
                        ObjectType = SyncBatchObjectTypeRef
                    }
                };
                await objectManager.DeleteAsync(_workspaceId, request).ConfigureAwait(false);
            }
        }

        [IdentifiedTest("7733524C-F1D2-49A2-899C-FE059AED59B3")]
        public async Task ItShouldCreateAllBatches()
        {
            // Arrange
            const int totalRecordsCount = 1000;
            const int batchSize = 100;
            ConfigurationStub configuration = new ConfigurationStub()
            {
                TotalRecordsCount = totalRecordsCount,
                SyncBatchSize = batchSize,
                ExportRunId = Guid.Empty,
                SourceWorkspaceArtifactId = _workspaceId,
                SyncConfigurationArtifactId = _syncConfigurationId
            };

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);

            List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

            const int expectedNumBatches = 10;
            batches.Count.Should().Be(expectedNumBatches);

            AssertBatchesInOrder(batches);
        }

        [IdentifiedTest("417014C2-8266-404A-ACCA-43FD512B0F31")]
        public async Task ItShouldCreateRemainingBatches()
        {
            // Arrange
            const int batchSize = 100;
            const int firstStartingIndex = 0;
            const int secondStartingIndex = firstStartingIndex + batchSize;
            const int thirdStartingIndex = secondStartingIndex + batchSize;
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, batchSize, firstStartingIndex).ConfigureAwait(false);
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, batchSize, secondStartingIndex).ConfigureAwait(false);
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, batchSize, thirdStartingIndex).ConfigureAwait(false);

            const int totalRecordsCount = 1000;
            ConfigurationStub configuration = new ConfigurationStub()
            {
                TotalRecordsCount = totalRecordsCount,
                SyncBatchSize = batchSize,
                ExportRunId = _EXPORT_RUN_ID,
                SourceWorkspaceArtifactId = _workspaceId,
                SyncConfigurationArtifactId = _syncConfigurationId
            };

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);

            List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

            const int expectedNumBatches = 10;
            batches.Count.Should().Be(expectedNumBatches);

            AssertBatchesInOrder(batches);
        }

        [IdentifiedTest("DB21EA39-48A7-448D-88A5-F3636020D401")]
        public async Task ItShouldCreateRemainingBatchesWithNewBatchSize()
        {
            // Arrange
            const int originalBatchSize = 100;
            const int firstStartingIndex = 0;
            const int secondStartingIndex = firstStartingIndex + originalBatchSize;
            const int thirdStartingIndex = secondStartingIndex + originalBatchSize;
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, originalBatchSize, firstStartingIndex).ConfigureAwait(false);
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, originalBatchSize, secondStartingIndex).ConfigureAwait(false);
            await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, _EXPORT_RUN_ID, originalBatchSize, thirdStartingIndex).ConfigureAwait(false);

            const int totalRecordsCount = 1000;
            const int newBatchSize = 350;
            ConfigurationStub configuration = new ConfigurationStub()
            {
                TotalRecordsCount = totalRecordsCount,
                SyncBatchSize = newBatchSize,
                ExportRunId = _EXPORT_RUN_ID,
                SourceWorkspaceArtifactId = _workspaceId,
                SyncConfigurationArtifactId = _syncConfigurationId
            };

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);

            List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

            const int expectedNumBatches = 5;
            batches.Count.Should().Be(expectedNumBatches);

            AssertBatchesInOrder(batches);
        }

        private static async Task<List<RelativityObject>> GetBatchesAsync(IServiceFactory serviceFactory, int workspaceId, int syncConfigurationId, int length = 100)
        {
            using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                var request = new QueryRequest
                {
                    ObjectType = SyncBatchObjectTypeRef,
                    Condition = $"'SyncConfiguration' == OBJECT {syncConfigurationId}",
                    Sorts = new[]
                    {
                        new Sort
                        {
                            FieldIdentifier = new FieldRef { Guid = StartingIndexGuid },
                            Direction = SortEnum.Ascending
                        }
                    },
                    Fields = new []
                    {
                        new FieldRef { Guid = StartingIndexGuid },
                        new FieldRef { Guid = TotalDocumentsCountGuid }
                    }
                };

                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, length).ConfigureAwait(false);
                return result.Objects;
            }
        }

        private static void AssertBatchesInOrder(IEnumerable<RelativityObject> batches)
        {
            int expectedStartIndex = 0;
            foreach (RelativityObject batch in batches)
            {
                batch[StartingIndexGuid].Value.Should().Be(expectedStartIndex);
                expectedStartIndex += (int)batch[TotalDocumentsCountGuid].Value;
            }
        }

    }
}