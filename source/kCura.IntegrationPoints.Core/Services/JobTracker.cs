using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobTracker
	{
		private readonly JobResoureTracker _tracker;
		public JobTracker(JobResoureTracker tracker)
		{
			_tracker = tracker;
		}

		public static string GenerateTableTempTableName(Job job, string batchID)
		{
			return string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, batchID);
		}

		public void CreateTrackingEntry(Job job, string batchId)
		{
			var table = GenerateTableTempTableName(job, batchId);
			_tracker.CreateTrackingEntry(table, job.JobId);
		}

		public bool CheckEntries(Job job, string batchId)
		{
			var table = GenerateTableTempTableName(job, batchId);
			return _tracker.RemoveEntryAndCheckStatus(table, job.JobId) == 0;
		}

	}
}
