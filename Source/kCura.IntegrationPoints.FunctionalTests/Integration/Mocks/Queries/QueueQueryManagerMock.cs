using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
    class QueueQueryManagerMock : IQueueQueryManager
    {
        private readonly RelativityInstanceTest _db;
        private readonly TestContext _context;
        private int _scheduleQueueCreateRequestCount;

        public QueueQueryManagerMock(RelativityInstanceTest database, TestContext context)
        {
            _db = database;
            _context = context;
        }

        public ICommand CreateScheduleQueueTable()
        {
            return new ActionCommand(() =>
            {
                ++_scheduleQueueCreateRequestCount;
            });
        }

        public ICommand AddCustomColumnsToQueueTable()
        {
            return ActionCommand.Empty;
        }

        public IQuery<DataRow> GetAgentTypeInformation(Guid agentGuid)
        {
            AgentTest agent = _db.Agents.First(x => x.AgentGuid == agentGuid);

            return new ValueReturnQuery<DataRow>(agent.AsRow());
        }

        public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, int[] resourceGroupArtifactId)
        {
            JobTest nextJob = _db.JobsInQueue.Where(x =>
                    x.AgentTypeID == agentTypeId &&
                    x.NextRunTime <= _context.CurrentDateTime &&
                    (x.StopState.HasFlag(StopState.None) || x.StopState.HasFlag(StopState.DrainStopped)))
                .OrderByDescending(x => x.StopState)
                .FirstOrDefault();

            if (nextJob == null)
            {
                return new ValueReturnQuery<DataTable>(null);
            }

            nextJob.LockedByAgentID = agentId;
            nextJob.StopState = 0;

            return new ValueReturnQuery<DataTable>(nextJob.AsTable());
        }

        public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, long? rootJobId)
        {
            JobTest nextJob = _db.JobsInQueue.Where(x =>
                    x.AgentTypeID == agentTypeId &&
                    x.NextRunTime <= _context.CurrentDateTime &&
                    (rootJobId == null || x.RootJobId == rootJobId) &&
                    (x.StopState.HasFlag(StopState.None) || x.StopState.HasFlag(StopState.DrainStopped)))
                .OrderByDescending(x => x.StopState)
                .FirstOrDefault();

            if (nextJob == null)
            {
                return new ValueReturnQuery<DataTable>(null);
            }

            nextJob.LockedByAgentID = agentId;
            nextJob.StopState = 0;

            return new ValueReturnQuery<DataTable>(nextJob.AsTable());
        }

        public ICommand UnlockScheduledJob(int agentId)
        {
            return new ActionCommand(() =>
            {
                JobTest lockedJob = _db.JobsInQueue.FirstOrDefault(x => x.LockedByAgentID == agentId);

                if (lockedJob != null)
                {
                    lockedJob.LockedByAgentID = null;
                }
            });
        }

        public ICommand UnlockJob(long jobId, StopState state)
        {
            return new ActionCommand(() =>
            {
                JobTest lockedJob = _db.JobsInQueue.FirstOrDefault(x => x.JobId == jobId);

                if (lockedJob != null)
                {
                    lockedJob.LockedByAgentID = null;
                    lockedJob.StopState = state;
                }
            });
        }

        public ICommand DeleteJob(long jobId)
        {
            return new ActionCommand(() =>
            {
                _db.JobsInQueue.RemoveAll(x => x.JobId == jobId);
            });
        }

        public IQuery<DataTable> CreateScheduledJob(int workspaceId, int relatedObjectArtifactId, string taskType, DateTime nextRunTime,
            int agentTypeId, string scheduleRuleType, string serializedScheduleRule, string jobDetails, int jobFlags,
            int submittedBy, long? rootJobId, long? parentJobId = null)
        {
            long newJobId = JobId.Next;

            JobTest newJob = CreateJob(newJobId, workspaceId, relatedObjectArtifactId, taskType,
                nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule,
                jobDetails, jobFlags, submittedBy, rootJobId, parentJobId);

            _db.JobsInQueue.Add(newJob);

            return new ValueReturnQuery<DataTable>(newJob.AsTable());
        }

        public ICommand CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID,
            string taskType, DateTime nextRunTime, int AgentTypeID, string scheduleRuleType, string serializedScheduleRule,
            string jobDetails, int jobFlags, int SubmittedBy, long? rootJobID, long? parentJobID = null)
        {
            return new ActionCommand(() =>
            {
                _db.JobsInQueue.RemoveAll(x => x.JobId == oldScheduledJobId);

                long newJobId = JobId.Next;

                JobTest newJob = CreateJob(newJobId, workspaceID, relatedObjectArtifactID, taskType,
                    nextRunTime, AgentTypeID, scheduleRuleType, serializedScheduleRule,
                    jobDetails, jobFlags, SubmittedBy, rootJobID, parentJobID);

                _db.JobsInQueue.Add(newJob);
            });
        }

        public ICommand CleanupJobQueueTable()
        {
            return new ActionCommand(() =>
            {
                IEnumerable<JobTest> jobs = _db
                    .JobsInQueue
                    .Where(x => _db.Agents.Exists(a => a.ArtifactId == x.LockedByAgentID));

                foreach (JobTest job in jobs)
                {
                    job.LockedByAgentID = null;
                }

                _db.JobsInQueue.RemoveAll(x => x.LockedByAgentID == null &&
                                               _db.Workspaces.FirstOrDefault(w => w.ArtifactId == x.WorkspaceID) == null);
            });
        }

        public ICommand CleanupScheduledJobsQueue()
        {
            return ActionCommand.Empty;
        }

        public IQuery<DataTable> GetAllJobs()
        {
            DataTable dataTable = DatabaseSchema.ScheduleQueueSchema();

            _db.JobsInQueue.ForEach(x => dataTable.ImportRow(x.AsDataRow()));

            return new ValueReturnQuery<DataTable>(dataTable);
        }

        public IQuery<int> UpdateStopState(IList<long> jobIds, StopState state)
        {
            int affectedRows = 0;
            IEnumerable<JobTest> jobs = _db.JobsInQueue.Where(x => jobIds.Contains(x.JobId));
            foreach (JobTest job in jobs)
            {
                ++affectedRows;
                job.StopState = state;
            }

            return new ValueReturnQuery<int>(affectedRows);
        }

        public IQuery<DataTable> GetJobByRelatedObjectIdAndTaskType(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
        {
            IEnumerable<JobTest> jobs = _db.JobsInQueue.Where(x =>
                x.WorkspaceID == workspaceId &&
                x.RelatedObjectArtifactID == relatedObjectArtifactId &&
                taskTypes.Contains(x.TaskType));

            DataTable dataTable = DatabaseSchema.ScheduleQueueSchema();

            foreach (JobTest job in jobs)
            {
                dataTable.ImportRow(job.AsDataRow());
            }

            return new ValueReturnQuery<DataTable>(dataTable);
        }

        public IQuery<DataTable> GetJobsByIntegrationPointId(long integrationPointId)
        {
            IEnumerable<JobTest> jobs = _db.JobsInQueue.Where(x => x.RelatedObjectArtifactID == integrationPointId);

            DataTable dataTable = DatabaseSchema.ScheduleQueueSchema();

            foreach (JobTest job in jobs)
            {
                dataTable.ImportRow(job.AsDataRow());
            }

            return new ValueReturnQuery<DataTable>(dataTable);
        }

        public IQuery<DataTable> GetJob(long jobId)
        {
            IEnumerable<JobTest> jobs = _db.JobsInQueue.Where(x => x.JobId == jobId);

            DataTable dataTable = DatabaseSchema.ScheduleQueueSchema();

            foreach (JobTest job in jobs)
            {
                dataTable.ImportRow(job.AsDataRow());
            }

            return new ValueReturnQuery<DataTable>(dataTable);
        }

        public ICommand UpdateJobDetails(long jobId, string jobDetails)
        {
            return new ActionCommand(() =>
            {
                JobTest job = _db.JobsInQueue.FirstOrDefault(x => x.JobId == jobId);

                if (job != null)
                {
                    job.JobDetails = jobDetails;
                }
            });
        }

        public IQuery<bool> CheckAllSyncWorkerBatchesAreFinished(long rootJobId)
        {
            bool tasksFinished = !_db.JobsInQueue.Exists(x => x.RootJobId == rootJobId && x.TaskType == "SyncWorker");

            return new ValueReturnQuery<bool>(tasksFinished);
        }

        public IQuery<int> Heartbeat(long jobId, DateTime heartbeatTime)
        {
            return new ValueReturnQuery<int>(0);
        }

        #region Test Verification

        public void ShouldCreateQueueTable()
        {
            _scheduleQueueCreateRequestCount.Should().BePositive();
        }

        #endregion

        #region Implementation Details
        private JobTest CreateJob(long jobId, int workspaceId, int relatedObjectArtifactId, string taskType,
            DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
            string jobDetails, int jobFlags, int submittedBy, long? rootJobId, long? parentJobId)
        {
            JobTest jobTest = new JobTest
            {
                JobId = jobId,
                RootJobId = rootJobId,
                ParentJobId = parentJobId,
                AgentTypeID = agentTypeId,
                LockedByAgentID = null,
                WorkspaceID = workspaceId,
                RelatedObjectArtifactID = relatedObjectArtifactId,
                TaskType = taskType,
                NextRunTime = nextRunTime,
                LastRunTime = null,
                ScheduleRuleType = scheduleRuleType,
                SerializedScheduleRule = serializedScheduleRule,
                JobDetails = jobDetails,
                JobFlags = jobFlags,
                SubmittedDate = _context.CurrentDateTime,
                SubmittedBy = submittedBy,
                StopState = _db.JobsInQueue.FirstOrDefault(x => x.JobId == parentJobId)?.StopState ?? StopState.None
            };

            return jobTest;
        }

        #endregion
    }
}
