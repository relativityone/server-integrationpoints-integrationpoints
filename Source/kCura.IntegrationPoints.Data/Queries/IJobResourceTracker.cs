namespace kCura.IntegrationPoints.Data.Queries
{
    public interface IJobResourceTracker
    {
        void CreateTrackingEntry(string tableName, long jobId, int workspaceId);

        int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceId, bool batchIsFinished);

        int GetProcessingBatchesCount(string tableName, long rootJobId, int workspaceId);
    }
}