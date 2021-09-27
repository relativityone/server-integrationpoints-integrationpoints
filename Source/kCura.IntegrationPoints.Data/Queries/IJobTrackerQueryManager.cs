using System.Data;

namespace kCura.IntegrationPoints.Data.Queries
{
	public interface IJobTrackerQueryManager
	{
		ICommand CreateJobTrackingEntry(string tableName, int workspaceId, long jobId);

		IQuery<int> RemoveEntryAndCheckBatchStatus(string tableName, int workspaceId, long jobId, bool isBatchFinished);

		IQuery<DataTable> GetJobIdsFromTrackingEntry(string tableName, int workspaceId, long rootJobId);
	}
}
