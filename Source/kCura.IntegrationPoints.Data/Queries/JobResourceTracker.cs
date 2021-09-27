using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.DTO;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobResourceTracker : IJobResourceTracker
	{
		private readonly IQueueQueryManager _queueQueryManager;
		private readonly IJobTrackerQueryManager _jobTrackerQueryManager;

		private static readonly object _syncRoot = new object();

		public JobResourceTracker(IJobTrackerQueryManager jobTrackerQueryManager, IQueueQueryManager queueQueryManager)
		{
			_jobTrackerQueryManager = jobTrackerQueryManager;
			_queueQueryManager = queueQueryManager;
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
				List<Job> jobs = new List<Job>();
				DataTable jobsDt = _queueQueryManager.GetAllJobs().Execute();
				foreach (DataRow jobRow in jobsDt.Rows)
				{
					Job job = new Job(jobRow);
					if (job.RootJobId == rootJobId)
					{
						jobs.Add(job);
					}
				}

				DataTable dataTable = _jobTrackerQueryManager.GetJobIdsFromTrackingEntry(tableName, workspaceId, rootJobId)
					.Execute();

				List<long> trackingEntryJobIds = new List<long>();
				foreach (DataRow row in dataTable.Rows)
				{
					trackingEntryJobIds.Add((long)row[0]);
				}

				var batchesStatuses = jobs.Join(trackingEntryJobIds, job => job.JobId, trackingEntryJobId => trackingEntryJobId,
					(job, entryTrackingId) => new { job.LockedByAgentID, job.StopState }).ToList();

				BatchStatusQueryResult result = new BatchStatusQueryResult
				{
					ProcessingCount = batchesStatuses.Count(x => x.LockedByAgentID != null),
					SuspendedCount = batchesStatuses.Count(x => x.LockedByAgentID == null && x.StopState == StopState.DrainStopped)
				};

				return result;
			}
		}
	}
}
