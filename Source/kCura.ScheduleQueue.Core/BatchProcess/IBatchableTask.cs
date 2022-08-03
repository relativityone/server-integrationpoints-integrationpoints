using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
    public interface IBatchableTask<T>
    {
        int BatchSize { get; }
        List<T> GetUnbatchedIDs(Job job);
        void CreateBatchJob(Job job, List<T> batchIDs);
    }
}
