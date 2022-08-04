using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    public interface ITaskFactoryJobHistoryService
    {
        void SetJobIdOnJobHistory(Job job);
        void UpdateJobHistoryOnFailure(Job job, Exception e);
        void RemoveJobHistoryFromIntegrationPoint(Job job);
    }
}
