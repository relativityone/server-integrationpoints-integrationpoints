

using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.Services.ServiceProxy;
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
        
        public Task CreateIntegrationPoint(IntegrationPointModel integrationPoint, int workspaceId)
        {
            using (var manager = _serviceFactory.GetServiceProxy<IIntegrationPointManager>())
            {
                
            }
            return Task.Delay(100);
            // using (IIntegrationPointManager integrationPointManager = _serviceFactory.CreateProxy<IIntegrationPointManager>())
            // {
            //     await integrationPointManager.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
            //     {
            //         IntegrationPoint = integrationPoint,
            //         
            //     });
            // }
        }

        public  Task<int> RunIntegrationPoint(IntegrationPointModel integrationPoint, int workspaceId)
        {
            _integrationPointRunningTask = Task.Delay(10000);
            return Task.FromResult(5);
            // using (IIntegrationPointManager integrationPointManager = _serviceFactory.CreateProxy<IIntegrationPointManager>())
            // {
            //     await integrationPointManager.RunIntegrationPointAsync(workspaceId, integrationPoint.ArtifactId);
            // }
        }

        public Task<string> CheckJobStatus(int jobId)
        {
            if (_integrationPointRunningTask.IsCompleted)
            {
                return Task.FromResult(JobStatusChoices.JobHistoryCompleted.Name);
            }

            return Task.FromResult(PerformanceTestsConstants.JOB_STATUS_PROCESSING);
        }
    }
}