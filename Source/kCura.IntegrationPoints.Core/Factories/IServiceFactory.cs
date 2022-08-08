using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IServiceFactory
    {
        IIntegrationPointService CreateIntegrationPointService(IHelper helper);
        
        IJobHistoryService CreateJobHistoryService(IAPILog logger);
    }
}