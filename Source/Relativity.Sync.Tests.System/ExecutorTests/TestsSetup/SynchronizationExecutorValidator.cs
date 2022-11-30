using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Extensions;

namespace Relativity.Sync.Tests.System.ExecutorTests.TestsSetup
{
    internal class SynchronizationExecutorValidator
    {
        public ConfigurationStub Configuration { get; }

        public ServiceFactory ServiceFactory { get; }

        private readonly Guid BatchObject = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private readonly Guid TransferredItemsCountField = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");

        public SynchronizationExecutorValidator(ConfigurationStub configuration, ServiceFactory serviceFactory)
        {
            Configuration = configuration;
            ServiceFactory = serviceFactory;
        }

        public void AssertResult(ExecutionResult result, ExecutionStatus expectedStatus)
        {
            Assert.AreEqual(expectedStatus, result.Status,
                message: AggregateJobHistoryErrorMessagesAsync(result, Configuration.SourceWorkspaceArtifactId, Configuration.JobHistoryArtifactId).GetAwaiter().GetResult());
        }

        public void AssertTotalTransferredItems(int expectedTotalCount)
        {
            Assert.AreEqual(expectedTotalCount,
                GetTotalTransferredItemsCountAsync().GetAwaiter().GetResult());
        }

        public void AssertTransferredItemsInBatches(IList<int> expectedTagCountInBatches)
        {
            IList<int> transferredItemsCountsPerBatch = GetTransferredItemsCountsPerBatchAsync().GetAwaiter().GetResult();

            CollectionAssert.IsNotEmpty(transferredItemsCountsPerBatch);
            CollectionAssert.AreEqual(transferredItemsCountsPerBatch, expectedTagCountInBatches);
        }

        #region Private Methods

        private async Task<string> AggregateJobHistoryErrorMessagesAsync(ExecutionResult syncResult, int workspaceId, int jobHistoryId)
        {
            using (var objectManager = await new ServiceFactoryStub(ServiceFactory).CreateProxyAsync<IObjectManager>()
                .ConfigureAwait(false))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Synchronization step failed: {syncResult.Message}: {syncResult.Exception}");
                sb.Append(await objectManager.AggregateJobHistoryErrorMessagesAsync(workspaceId, jobHistoryId)
                    .ConfigureAwait(false));

                return sb.ToString();
            }
        }

        private async Task<int> GetTotalTransferredItemsCountAsync()
        {
            IList<int> batchesTransferredItemsCounts = await GetTransferredItemsCountsPerBatchAsync().ConfigureAwait(false);

            return batchesTransferredItemsCounts.Sum();
        }

        private async Task<IList<int>> GetTransferredItemsCountsPerBatchAsync()
        {
            List<int> batchesTransferredItemsCounts = new List<int>();

            var serviceFactory = new ServiceFactoryStub(ServiceFactory);

            using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var batchesArtifactsIdsQueryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObject
                    },
                    Condition = $"'SyncConfiguration' == OBJECT {Configuration.SyncConfigurationArtifactId}"
                };

                QueryResultSlim batchesArtifactsIdsQueryResult = await objectManager
                    .QuerySlimAsync(Configuration.SourceWorkspaceArtifactId, batchesArtifactsIdsQueryRequest, 1, int.MaxValue).ConfigureAwait(false);
                if (batchesArtifactsIdsQueryResult.TotalCount > 0)
                {
                    IEnumerable<int> batchesArtifactsIds = batchesArtifactsIdsQueryResult.Objects.Select(x => x.ArtifactID);

                    foreach (int batchArtifactId in batchesArtifactsIds)
                    {
                        QueryRequest transferredItemsCountQueryRequest = new QueryRequest
                        {
                            ObjectType = new ObjectTypeRef
                            {
                                Guid = BatchObject
                            },
                            Fields = new[]
                            {
                                new FieldRef
                                {
                                    Guid = TransferredItemsCountField
                                }
                            },
                            Condition = $"'ArtifactID' == {batchArtifactId}"
                        };
                        QueryResult transferredItemsCountQueryResult = await objectManager
                            .QueryAsync(Configuration.SourceWorkspaceArtifactId, transferredItemsCountQueryRequest, 0, 1).ConfigureAwait(false);

                        batchesTransferredItemsCounts.Add((int)(transferredItemsCountQueryResult.Objects.Single()[TransferredItemsCountField].Value ?? default(int)));
                    }
;
                }
            }

            return batchesTransferredItemsCounts;
        }
    }

    #endregion
}
