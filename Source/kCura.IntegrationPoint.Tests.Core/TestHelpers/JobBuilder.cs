﻿using System;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public class JobBuilder
    {
        private const string _JOB_ID = "JobID";
        private const string _ROOT_JOB_ID = "RootJobId";
        private const string _PARENT_JOB_ID = "ParentJobId";
        private const string _AGENT_TYPE_ID = "AgentTypeID";
        private const string _LOCKED_BY_AGENT_ID = "LockedByAgentID";
        private const string _WORKSPACE_ID = "WorkspaceID";
        private const string _RELATED_OBJECT_ARTIFACT_ID = "RelatedObjectArtifactID";
        private const string _TASK_TYPE = "TaskType";
        private const string _NEXT_RUN_TIME = "NextRunTime";
        private const string _LAST_RUN_TIME = "LastRunTime";
        private const string _JOB_DETAILS = "JobDetails";
        private const string _JOB_FLAGS = "JobFlags";
        private const string _SUBMITTED_DATE = "SubmittedDate";
        private const string _SUBMITTED_BY = "SubmittedBy";
        private const string _SCHEDULE_RULE_TYPE = "ScheduleRuleType";
        private const string _SCHEDULE_RULE = "ScheduleRule";
        private const string _STOP_STATE = "StopState";
        private const string _HEARTBEAT = "Heartbeat";
        private readonly ISerializer _serializer = IntegrationPointSerializer.CreateWithoutLogger();
        private IsJobFailed _jobFailed;
        private DataRow _jobData;

        public JobBuilder()
        {
            InitializeWithDefaultData();
        }

        public Job Build()
        {
            Job job = new Job(_jobData);
            if (_jobFailed != null)
            {
                job.MarkJobAsFailed(_jobFailed.Exception, _jobFailed.ShouldBreakSchedule, _jobFailed.MaximumConsecutiveFailuresReached);
            }

            return job;
        }

        public JobBuilder WithJob(Job job)
        {
            CopyValuesFromJob(job);
            return this;
        }

        public JobBuilder WithJobId(long jobId)
        {
            _jobData[_JOB_ID] = jobId;
            return this;
        }

        public JobBuilder WithRootJobId(long? rootJobId)
        {
            _jobData[_ROOT_JOB_ID] = (object)rootJobId ?? DBNull.Value;
            return this;
        }

        public JobBuilder WithWorkspaceId(int workspaceId)
        {
            _jobData[_WORKSPACE_ID] = workspaceId;
            return this;
        }

        public JobBuilder WithJobDetails(TaskParameters jobDetails)
        {
            _jobData[_JOB_DETAILS] = _serializer.Serialize(jobDetails);
            return this;
        }

        public JobBuilder WithJobDetails(string jobDetails)
        {
            _jobData[_JOB_DETAILS] = jobDetails;
            return this;
        }

        public JobBuilder WithStopState(StopState state)
        {
            _jobData[_STOP_STATE] = (int)state;
            return this;
        }

        public JobBuilder WithTaskType(TaskType taskType)
        {
            _jobData[_TASK_TYPE] = taskType.ToString();
            return this;
        }

        public JobBuilder WithRelatedObjectArtifactId(int relatedObjectArtifactID)
        {
            _jobData[_RELATED_OBJECT_ARTIFACT_ID] = relatedObjectArtifactID;
            return this;
        }

        public JobBuilder WithSubmittedBy(int submittedByArtifactId)
        {
            _jobData[_SUBMITTED_BY] = submittedByArtifactId;
            return this;
        }

        public JobBuilder WithScheduleRuleType(string scheduleRuleType)
        {
            _jobData[_SCHEDULE_RULE_TYPE] = scheduleRuleType;
            return this;
        }

        public JobBuilder WithScheduleRule(IScheduleRule rule)
        {
            _jobData[_SCHEDULE_RULE] = rule.ToSerializedString();
            return this;
        }

        public JobBuilder WithLockedByAgentId(int? lockedByAgentId)
        {
            _jobData[_LOCKED_BY_AGENT_ID] = (object)lockedByAgentId ?? DBNull.Value;
            return this;
        }

        public JobBuilder WithHeartbeat(DateTime heartbeat)
        {
            _jobData[_HEARTBEAT] = heartbeat;
            return this;
        }

        public JobBuilder WithJobFailed(IsJobFailed isJobFailed)
        {
            _jobFailed = isJobFailed;
            return this;
        }

        private void InitializeWithDefaultData()
        {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn(_JOB_ID, typeof(long)));
            table.Columns.Add(new DataColumn(_ROOT_JOB_ID, typeof(long)));
            table.Columns.Add(new DataColumn(_PARENT_JOB_ID, typeof(long)));
            table.Columns.Add(new DataColumn(_AGENT_TYPE_ID, typeof(int)));
            table.Columns.Add(new DataColumn(_LOCKED_BY_AGENT_ID, typeof(int)));
            table.Columns.Add(new DataColumn(_WORKSPACE_ID, typeof(int)));
            table.Columns.Add(new DataColumn(_RELATED_OBJECT_ARTIFACT_ID, typeof(int)));
            table.Columns.Add(new DataColumn(_TASK_TYPE, typeof(string)));
            table.Columns.Add(new DataColumn(_NEXT_RUN_TIME, typeof(DateTime)));
            table.Columns.Add(new DataColumn(_LAST_RUN_TIME, typeof(DateTime)));
            table.Columns.Add(new DataColumn(_JOB_DETAILS, typeof(string)));
            table.Columns.Add(new DataColumn(_JOB_FLAGS, typeof(int)));
            table.Columns.Add(new DataColumn(_SUBMITTED_DATE, typeof(DateTime)));
            table.Columns.Add(new DataColumn(_SUBMITTED_BY, typeof(int)));
            table.Columns.Add(new DataColumn(_SCHEDULE_RULE_TYPE, typeof(string)));
            table.Columns.Add(new DataColumn(_SCHEDULE_RULE, typeof(string)));
            table.Columns.Add(new DataColumn(_STOP_STATE, typeof(int)));
            table.Columns.Add(new DataColumn(_HEARTBEAT, typeof(DateTime)));

            _jobData = table.NewRow();
            _jobData[_JOB_ID] = default(long);
            _jobData[_ROOT_JOB_ID] = default(long);
            _jobData[_PARENT_JOB_ID] = default(long);
            _jobData[_AGENT_TYPE_ID] = default(int);
            _jobData[_LOCKED_BY_AGENT_ID] = default(int);
            _jobData[_WORKSPACE_ID] = default(int);
            _jobData[_RELATED_OBJECT_ARTIFACT_ID] = default(int);
            _jobData[_TASK_TYPE] = TaskType.ExportService.ToString();
            _jobData[_NEXT_RUN_TIME] = default(DateTime);
            _jobData[_LAST_RUN_TIME] = default(DateTime);
            _jobData[_JOB_DETAILS] = _serializer.Serialize(new TaskParameters { BatchInstance = Guid.NewGuid() });
            _jobData[_JOB_FLAGS] = default(int);
            _jobData[_SUBMITTED_DATE] = default(DateTime);
            _jobData[_SUBMITTED_BY] = default(int);
            _jobData[_SCHEDULE_RULE_TYPE] = default(string);
            _jobData[_SCHEDULE_RULE] = default(string);
            _jobData[_STOP_STATE] = default(int);
            _jobData[_HEARTBEAT] = default(DateTime);
        }

        private void CopyValuesFromJob(Job job)
        {
            _jobData[_JOB_ID] = job.JobId;
            _jobData[_ROOT_JOB_ID] = job.RootJobId;
            _jobData[_PARENT_JOB_ID] = job.ParentJobId;
            _jobData[_AGENT_TYPE_ID] = job.AgentTypeID;
            _jobData[_LOCKED_BY_AGENT_ID] = job.LockedByAgentID;
            _jobData[_WORKSPACE_ID] = job.WorkspaceID;
            _jobData[_RELATED_OBJECT_ARTIFACT_ID] = job.RelatedObjectArtifactID;
            _jobData[_TASK_TYPE] = job.TaskType;
            _jobData[_NEXT_RUN_TIME] = job.NextRunTime;
            _jobData[_LAST_RUN_TIME] = (object)job.LastRunTime ?? DBNull.Value;
            _jobData[_JOB_DETAILS] = job.JobDetails;
            _jobData[_JOB_FLAGS] = job.JobFlags;
            _jobData[_SUBMITTED_DATE] = job.SubmittedDate;
            _jobData[_SUBMITTED_BY] = job.SubmittedBy;
            _jobData[_SCHEDULE_RULE_TYPE] = job.ScheduleRuleType;
            _jobData[_SCHEDULE_RULE] = job.ScheduleRule;
            _jobData[_STOP_STATE] = (int)job.StopState;
            _jobData[_HEARTBEAT] = (object)job.Heartbeat ?? DBNull.Value;
        }
    }
}
