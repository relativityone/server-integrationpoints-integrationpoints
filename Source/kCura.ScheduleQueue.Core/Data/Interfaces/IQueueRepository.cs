using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Data.Interfaces
{
    public interface IQueueRepository
    {
        long AddJob(Job job);

        Job GetJob(long jobId);

        IList<Job> GetAllJobs();

        bool UpdateJob(Job job);

        bool DeleteJob(long jobId);
    }
}
