namespace kCura.IntegrationPoints.Data.Queries
{
    public interface IJobResourceTracker
    {
        void CreateTrackingEntry(string tableName, long jobId, int workspaceID);
        int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceID);
    }
}