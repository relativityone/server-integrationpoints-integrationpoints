using System;
using System.Data;
using kCura.IntegrationPoints.Data.DTO;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobResourceTracker : IJobResourceTracker
    {
		private readonly IJobTrackerQueryManager _jobTrackerQueryManager;

		private static readonly object _syncRoot = new object();

		public JobResourceTracker(IJobTrackerQueryManager jobTrackerQueryManager)
		{
			_jobTrackerQueryManager = jobTrackerQueryManager;
		}

		public void CreateTrackingEntry(string tableName, long jobId, int workspaceId)
        {
	        lock (_syncRoot)
	        {
				_jobTrackerQueryManager.CreateJobTrackingEntry(tableName, workspaceId, jobId)
					.Execute();
	        }
        }

        public int RemoveEntryAndCheckStatus(string tableName, long jobId, int workspaceId, bool batchIsFinished)
        {
	        lock (_syncRoot)
	        {
				return _jobTrackerQueryManager.RemoveEntryAndCheckBatchStatus(tableName, workspaceId, jobId, batchIsFinished)
					.Execute();
	        }
        }

        public BatchStatusQueryResult GetBatchesStatuses(string tableName, long rootJobId, int workspaceId)
        {
	        lock (_syncRoot)
	        {
				DataTable dataTable = _jobTrackerQueryManager.GetProcessingSyncWorkerBatches(tableName, workspaceId, rootJobId)
					.Execute();

				BatchStatusQueryResult result = new BatchStatusQueryResult
		        {
					ProcessingCount = CountProcessingBatches(dataTable),
					SuspendedCount = CountSuspendedBatches(dataTable)
		        };
				
				return result;
	        }
        }

        private int CountSuspendedBatches(DataTable dataTable)
        {
	        const int stopStateSuspended = 8;
			int result = 0;
			
			foreach (DataRow row in dataTable.Rows)
	        {
		        
		        if (row[0] == DBNull.Value && (int)row[1] == stopStateSuspended)
		        {
			        ++result;
		        }
	        }

	        return result;
        }

        private int CountProcessingBatches(DataTable dataTable)
        {
	        int result = 0;

	        foreach (DataRow row in dataTable.Rows)
	        {
		        if (row[0] != DBNull.Value)
		        {
			        ++result;
		        }
	        }

	        return result;
        }
    }
}
