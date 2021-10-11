using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework.Api.Kepler;

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

            QueryRequest query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.JobHistoryGuid
                },
                Fields = new[] {new FieldRef {Name = "Integration Point"}}
            };

            using (var objectManager = _serviceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1000)
                    .ConfigureAwait(false);

                RelativityObject jobHistory = result.Objects
                    .Where(x => (x.FieldValues.First().Value as List<RelativityObjectValue>)?.First()?.ArtifactID ==
                                integrationPoint.ArtifactId)
                    .OrderByDescending(x => x.ArtifactID)
                    .First();

                return jobHistory.ArtifactID;
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
                Fields = new[] {new FieldRef {Name = "Job Status"}},
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
    }
}