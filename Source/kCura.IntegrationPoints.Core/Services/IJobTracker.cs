using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IJobTracker
    {
        void CreateTrackingEntry(Job job, string batchId);

        bool CheckEntries(Job job, string batchId, bool batchIsFinished);

        BatchStatusQueryResult GetBatchesStatuses(Job job, string batchId);
    }
}