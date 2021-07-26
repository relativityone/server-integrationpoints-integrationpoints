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
        private Task _integrationPointRunningTask;

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
                await  manager.RunIntegrationPointAsync(workspaceId, integrationPoint.ArtifactId).ConfigureAwait(false);
            }

            _integrationPointRunningTask = Task.Delay(10000);
            
            return 5;
        }

        public async Task<string> GetJobHistoryStatus(int integrationPointId, int workspaceId)
        {
            QueryRequest query = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.JobHistoryGuid
                },
                Fields = new []{new FieldRef{Name = "JobStatus"}},
                Condition = $"'ParentObject' == '{integrationPointId}'"
            };

            using (var objectManager = _serviceFactory
                .GetServiceProxy<IObjectManager>())
            {
                var result = await objectManager.QueryAsync(workspaceId, query, 0, 1000)
                    .ConfigureAwait(false);

                return result.Objects.OrderByDescending(x => x.ArtifactID).First().FieldValues.First().Value.ToString();
            }
        }
    }
}