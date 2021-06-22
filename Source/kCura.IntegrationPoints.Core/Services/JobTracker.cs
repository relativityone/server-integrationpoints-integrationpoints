﻿using System;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services
{
    public class JobTracker : IJobTracker
    {
        private readonly IJobResourceTracker _tracker;

        public JobTracker(IJobResourceTracker tracker)
        {
            _tracker = tracker;
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

            _tracker.CreateTrackingEntry(jobTrackerTempTableName, job.JobId, job.WorkspaceID);
        }

        public bool CheckEntries(Job job, string batchId, bool batchIsFinished)
        {
	        string jobTrackerTempTableName = GenerateJobTrackerTempTableName(job, batchId);

            return _tracker.RemoveEntryAndCheckStatus(jobTrackerTempTableName, job.JobId, job.WorkspaceID, batchIsFinished) == 0;
        }

        public int GetProcessingBatchesCount(Job job, string batchId)
        {
	        string jobTrackerTempTableName = GenerateJobTrackerTempTableName(job, batchId);

	        return _tracker.GetProcessingBatchesCount(jobTrackerTempTableName, job.RootJobId.Value, job.WorkspaceID);
        }
    }
}
