using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    public interface ITaskFactoryJobHistoryServiceFactory
    {
        ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPointDto integrationPoint);
    }
}
