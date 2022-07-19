using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal sealed class ProgressTests : SystemTest
    {
        private WorkspaceRef _sourceWorkspace;

        private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

        private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
        private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
        private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
        private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");

        [SetUp]
        public async Task SetUp()
        {
            _sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
        }

        [IdentifiedTestCase("1be9ae3e-c78f-408c-a06e-1d9359114d41", typeof(SyncDocumentRunPipeline))]
        [IdentifiedTestCase("0e424672-33f6-46cb-99df-5e30a6e7e897", typeof(SyncDocumentRetryPipeline))]
        public async Task ItShouldLogProgressForEachStep(Type pipelineType)
        {
            int workspaceArtifactId = _sourceWorkspace.ArtifactID;
            string jobHistoryName = $"JobHistory.{Guid.NewGuid()}";

            int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, workspaceArtifactId, jobHistoryName).ConfigureAwait(false);
            ConfigurationStub configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = workspaceArtifactId,
                JobHistoryArtifactId = jobHistoryArtifactId,
                JobHistoryToRetryId = pipelineType == typeof(SyncDocumentRetryPipeline) ? (int?)1 : null
            };

            configuration.SyncConfigurationArtifactId = await Rdos
                .CreateSyncConfigurationRdoAsync(_sourceWorkspace.ArtifactID, configuration).ConfigureAwait(false);
            
            ISyncJob syncJob = SyncJobHelper.CreateWithMockedAllSteps(configuration);

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            List<RelativityObject> progressRdos = await QueryForProgressRdosAsync(workspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);

            const int nonNodeProgressSteps = 1; // MultiNode
            int minimumExpectedProgressRdos = GetSyncNodes(pipelineType).Count + nonNodeProgressSteps;
            progressRdos.Count.Should().Be(minimumExpectedProgressRdos);
        }

        private async Task<List<RelativityObject>> QueryForProgressRdosAsync(int workspaceId, int syncConfigurationId)
        {
            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = ProgressObjectTypeGuid
                    },
                    Condition = $"'SyncConfiguration' == {syncConfigurationId}",
                    Fields = new[]
                    {
                        new FieldRef
                        {
                            Guid = OrderGuid
                        },
                        new FieldRef
                        {
                            Guid = StatusGuid
                        },
                        new FieldRef
                        {
                            Guid = ExceptionGuid
                        },
                        new FieldRef
                        {
                            Guid = MessageGuid
                        }
                    }
                };

                const int maxNumResults = 100;
                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 1, maxNumResults).ConfigureAwait(false);
                return result.Objects;
            }
        }

        private static List<Type> GetSyncNodes(Type pipelineType)
        {
            return PipelinesNodeHelper.GetExpectedNodesInExecutionOrder(pipelineType).Select(x => x.First()).ToList();
        }
    }
}
