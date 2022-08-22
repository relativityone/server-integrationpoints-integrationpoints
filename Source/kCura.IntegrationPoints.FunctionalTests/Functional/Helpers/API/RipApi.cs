using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework.Api.Kepler;
using WorkloadDiscovery;
using Choice = Relativity.Services.Objects.DataContracts.Choice;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    public class RipApi : IRipApi
    {
        private readonly IKeplerServiceFactory _serviceFactory;

        public RipApi(IKeplerServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task CreateIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId)
        {
            using (var manager = _serviceFactory.GetServiceProxy<IIntegrationPointManager>())
            {
                IntegrationPointModel result = await manager.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
                {
                    IntegrationPoint = integrationPoint,
                    WorkspaceArtifactId = workspaceId
                }).ConfigureAwait(false);

                integrationPoint.ArtifactId = result.ArtifactId;
            }
        }

        public async Task<int> RunIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId)
        {
            using (var manager = _serviceFactory.GetServiceProxy<IIntegrationPointManager>())
            {
                await manager.RunIntegrationPointAsync(workspaceId, integrationPoint.ArtifactId).ConfigureAwait(false);
            }

            return await GetJobHistoryId(integrationPoint.Name, workspaceId);
        }

        public async Task<int> RetryIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId, bool switchToAppendOverlayMode)
        {
            using (var manager = _serviceFactory.GetServiceProxy<IIntegrationPointManager>())
            {
                await manager.RetryIntegrationPointAsync(workspaceId, integrationPoint.ArtifactId, switchToAppendOverlayMode).ConfigureAwait(false);
            }

            return await GetJobHistoryId(integrationPoint.Name, workspaceId);
        }

        public async Task<int> GetJobHistoryId(string integrationPointName, int workspaceId)
        {
            QueryRequest query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.JobHistoryGuid
                },
                Fields = new FieldRef[] { new FieldRef { Name = "Job Status" } },
                Condition = $"'Name' LIKE '{integrationPointName}'"
            };

            using (var objectManager = _serviceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, int.MaxValue)
                    .ConfigureAwait(false);

                return result.Objects.OrderByDescending(x => x.ArtifactID).FirstOrDefault().ArtifactID;
            }
        }

        public async Task<string> GetJobHistoryStatus(int jobHistoryId, int workspaceId)
        {
            QueryRequest query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.JobHistoryGuid
                },
                Fields = new[] { new FieldRef { Name = "Job Status" } },
                Condition = $"'ArtifactId' == '{jobHistoryId}'"
            };

            using (var objectManager = _serviceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1)
                    .ConfigureAwait(false);

                return (result.Objects.FirstOrDefault()?.FieldValues.FirstOrDefault()?.Value as Choice)?.Name;
            }
        }

        public async Task WaitForJobToFinishAsync(int jobHistoryId, int workspaceId, int checkDelayInMs = 500, string expectedStatus = "Completed")
        {
            Task waitForJobStatus = Task.Run(() => WaitForJobStatus(jobHistoryId, workspaceId, status =>
                status == expectedStatus, checkDelayInMs));

            int waitingTimeout = 300;
            if (!waitForJobStatus.Wait(TimeSpan.FromSeconds(waitingTimeout)))
            {
                string status = await GetJobHistoryStatus(jobHistoryId, workspaceId).ConfigureAwait(false);

                throw new TimeoutException($"Waiting for job to finish timeout ({waitingTimeout}) exceeded. - JobStatus is {status}");
            }
        }

        public async Task<Workload> GetWorkloadSizeAsync()
        {
            Workload workloadSizeReturned = null;
            using (var integrationPointAgentManager = _serviceFactory.GetServiceProxy<IIntegrationPointsAgentManager>())
            {
                workloadSizeReturned = await integrationPointAgentManager.GetWorkloadAsync().ConfigureAwait(false);
            }

            return workloadSizeReturned;
        }

        private async Task WaitForJobStatus(int jobHistoryId, int workspaceId, Func<string, bool> waitUntil, int checkDelayInMs)
        {
            string status = await GetJobHistoryStatus(jobHistoryId, workspaceId);
            while (!waitUntil(status))
            {
                await Task.Delay(checkDelayInMs);
                status = await GetJobHistoryStatus(jobHistoryId, workspaceId).ConfigureAwait(false);

                if (status == JobStatusChoices.JobHistoryErrorJobFailed.Name
                    || status == JobStatusChoices.JobHistoryValidationFailed.Name)
                {
                    throw new InvalidOperationException($"Job failed with status {status}");
                }
            }
        }
    }
}