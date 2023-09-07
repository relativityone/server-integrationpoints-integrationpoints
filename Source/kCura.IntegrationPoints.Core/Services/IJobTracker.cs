using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IJobTracker
    {
        void CreateTrackingEntry(Job job);

        bool CheckEntries(Job job, bool batchIsFinished);

        BatchStatusQueryResult GetBatchesStatuses(Job job);
    }
}
