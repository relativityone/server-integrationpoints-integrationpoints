using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    public interface ITaskFactoryJobHistoryServiceFactory
    {
        ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPoint integrationPoint);
    }
}
