using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobTracker : IJobTracker
    {
        private readonly IJobResourceTracker _tracker;
        private readonly IAPILog _logger;

        public JobTracker(IJobResourceTracker tracker, IAPILog logger)
        {
            _tracker = tracker;
            _logger = logger;
        }

        public static string GenerateJobTrackerTempTableName(Job job, string batchID)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            return string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, batchID);
        }

        public void CreateTrackingEntry(Job job, string batchId)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            string jobTrackerTempTableName = GenerateJobTrackerTempTableName(job, batchId);
            _logger.LogInformation("Creating job entry {jobTrackerTempTableName}", jobTrackerTempTableName);

            _tracker.CreateTrackingEntry(jobTrackerTempTableName, job.JobId, job.WorkspaceID);
        }

        public bool CheckEntries(Job job, string batchId, bool batchIsFinished)
        {
	        string jobTrackerTempTableName = GenerateJobTrackerTempTableName(job, batchId);
            _logger.LogInformation("JobTracker RemoveEntryAndCheckStatus for {jobTrackerTempTableName}", jobTrackerTempTableName);

            return _tracker.RemoveEntryAndCheckStatus(jobTrackerTempTableName, job.JobId, job.WorkspaceID, batchIsFinished) == 0;
        }

        public BatchStatusQueryResult GetBatchesStatuses(Job job, string batchId)
        {
	        string jobTrackerTempTableName = GenerateJobTrackerTempTableName(job, batchId);

	        return _tracker.GetBatchesStatuses(jobTrackerTempTableName, job.RootJobId.Value, job.WorkspaceID);
        }
    }
}
