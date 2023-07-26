using System;
using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class JobHelper
    {
        public static Job CreateJob(
            long jobId,
            long? rootJobId,
            long? parentJobId,
            int agentTypeId,
            int lockedByAgentId,
            int workspaceId,
            int relatedObjectArtifactId,
            Guid correlationId,
            TaskType taskType,
            DateTime nextRunTime,
            DateTime? lastRunTime,
            string jobDetails,
            int jobFlags,
            DateTime submittedDate,
            int submittedBy,
            string scheduleRuleType,
            string serializedScheduleRule,
            StopState stopState
            )
        {
            DataTable dt = CreateEmptyJobDataTable();
            DataRow row = CreateJobDataRow(jobId, rootJobId, parentJobId, agentTypeId, lockedByAgentId, workspaceId,
                relatedObjectArtifactId, correlationId, taskType, nextRunTime, lastRunTime, jobDetails, jobFlags,
                submittedDate, submittedBy, scheduleRuleType, serializedScheduleRule, stopState, dt);
            dt.Rows.Add(row);
            return new Job(row);
        }

        public static DataRow CreateJobDataRow(long jobId, long? rootJobId, long? parentJobId, int agentTypeId,
            int? lockedByAgentId, int workspaceId, int relatedObjectArtifactId, Guid correlationId, TaskType taskType, DateTime nextRunTime,
            DateTime? lastRunTime, string jobDetails, int jobFlags, DateTime submittedDate, int submittedBy,
            string scheduleRuleType, string serializedScheduleRule, StopState stopState, DataTable dt = null)
        {
            dt = dt ?? CreateEmptyJobDataTable();

            DataRow row = dt.NewRow();
            row["JobID"] = jobId;
            if (rootJobId.HasValue) row["RootJobID"] = rootJobId;
            else row["RootJobID"] = DBNull.Value;
            if (parentJobId.HasValue) row["ParentJobID"] = parentJobId;
            else row["ParentJobID"] = DBNull.Value;
            row["AgentTypeID"] = agentTypeId;
            row["LockedByAgentID"] = (object)lockedByAgentId ?? DBNull.Value;
            row["WorkspaceID"] = workspaceId;
            row["RelatedObjectArtifactID"] = relatedObjectArtifactId;
            row["CorrelationID"] = correlationId;
            row["TaskType"] = taskType.ToString();
            row["NextRunTime"] = nextRunTime;
            if (lastRunTime.HasValue) row["LastRunTime"] = lastRunTime.Value;
            else row["LastRunTime"] = DBNull.Value;
            row["JobDetails"] = jobDetails;
            row["JobFlags"] = jobFlags;
            row["SubmittedDate"] = submittedDate;
            row["SubmittedBy"] = submittedBy;
            row["ScheduleRuleType"] = scheduleRuleType;
            row["ScheduleRule"] = serializedScheduleRule;
            row["StopState"] = stopState;
            row["Heartbeat"] = DBNull.Value;
            return row;
        }

        public static DataTable CreateEmptyJobDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn() {ColumnName = "JobID", DataType = typeof(long) },
                new DataColumn() {ColumnName = "RootJobID", DataType = typeof(long), AllowDBNull = true },
                new DataColumn() {ColumnName = "ParentJobID", DataType = typeof(long), AllowDBNull = true },
                new DataColumn() {ColumnName = "AgentTypeID", DataType = typeof(int) },
                new DataColumn() {ColumnName = "LockedByAgentID", DataType = typeof(int) },
                new DataColumn() {ColumnName = "WorkspaceID", DataType = typeof(int) },
                new DataColumn() {ColumnName = "RelatedObjectArtifactID", DataType = typeof(int) },
                new DataColumn() {ColumnName = "CorrelationID", DataType = typeof(Guid) },
                new DataColumn() {ColumnName = "TaskType", DataType = typeof(string) },
                new DataColumn() {ColumnName = "NextRunTime", DataType = typeof(DateTime) },
                new DataColumn() {ColumnName = "LastRunTime", DataType = typeof(DateTime), AllowDBNull = true },
                new DataColumn() {ColumnName = "JobDetails", DataType = typeof(string) },
                new DataColumn() {ColumnName = "JobFlags", DataType = typeof(int) },
                new DataColumn() {ColumnName = "SubmittedDate", DataType = typeof(DateTime) },
                new DataColumn() {ColumnName = "SubmittedBy", DataType = typeof(int) },
                new DataColumn() {ColumnName = "ScheduleRuleType", DataType = typeof(string) },
                new DataColumn() {ColumnName = "ScheduleRule", DataType = typeof(string) },
                new DataColumn() {ColumnName = "StopState", DataType = typeof(int) },
                new DataColumn() {ColumnName = "Heartbeat", DataType = typeof(DateTime), AllowDBNull = true },
            });
            return dt;
        }

        public static Job GetJob(
            long jobId,
            long? rootJobId,
            long? parentJobId,
            int agentTypeId,
            int lockedByAgentId,
            int workspaceId,
            int relatedObjectArtifactId,
            Guid correlationId,
            TaskType taskType,
            DateTime nextRunTime,
            DateTime? lastRunTime,
            string jobDetails,
            int jobFlags,
            DateTime submittedDate,
            int submittedBy,
            string scheduleRuleType,
            string serializedScheduleRule)
        {
            return CreateJob(jobId, rootJobId, parentJobId, agentTypeId, lockedByAgentId, workspaceId, relatedObjectArtifactId,
                correlationId, taskType, nextRunTime, lastRunTime, jobDetails, jobFlags, submittedDate, submittedBy, 
                scheduleRuleType, serializedScheduleRule, StopState.None);
        }

        public static Job GetFakeJobOfTaskType(TaskType taskType)
        {
            return CreateJob(1, 2, 3, 4, 5, 6,
                7, Guid.NewGuid(), taskType, DateTime.MinValue,
                DateTime.MinValue, null, 1, DateTime.MinValue, 2,
                "", null, StopState.None);
        }
    }
}
