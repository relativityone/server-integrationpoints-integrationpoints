using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Data.Interfaces
{
    public interface IQueueRepository
    {
        Job AddJob(Job job);

        Job GetJob(int jobId);

        Job GetJob(int relatedObjectId, string taskType);

        IList<Job> GetAllJobs();

        bool UpdateJob(Job job);

        bool DeleteJob(int jobId);

        void CleanupQueue();

        void CleanupQueue(string agentGuid);

        bool AllSyncWorkerBatchesAreFinished(int rootJobId);
    }
}
