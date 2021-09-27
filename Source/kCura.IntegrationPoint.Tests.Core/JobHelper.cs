using System;
using System.Data;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

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
		        relatedObjectArtifactId, taskType, nextRunTime, lastRunTime, jobDetails, jobFlags, submittedDate, submittedBy,
		        scheduleRuleType, serializedScheduleRule, stopState, dt);
		    dt.Rows.Add(row);
			return new Job(row);
		}

	    public static DataRow CreateJobDataRow(long jobId, long? rootJobId, long? parentJobId, int agentTypeId,
	        int? lockedByAgentId, int workspaceId, int relatedObjectArtifactId, TaskType taskType, DateTime nextRunTime,
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
	        return row;
	    }

	    public static DataTable CreateEmptyJobDataTable()
	    {
	        DataTable dt = new DataTable();
	        dt.Columns.AddRange(new DataColumn[]
	        {
	            new DataColumn() {ColumnName = "JobID", DataType = typeof(long)},
	            new DataColumn() {ColumnName = "RootJobID", DataType = typeof(long), AllowDBNull = true},
	            new DataColumn() {ColumnName = "ParentJobID", DataType = typeof(long), AllowDBNull = true},
	            new DataColumn() {ColumnName = "AgentTypeID", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "LockedByAgentID", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "WorkspaceID", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "RelatedObjectArtifactID", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "TaskType", DataType = typeof(string)},
	            new DataColumn() {ColumnName = "NextRunTime", DataType = typeof(DateTime)},
	            new DataColumn() {ColumnName = "LastRunTime", DataType = typeof(DateTime), AllowDBNull = true},
	            new DataColumn() {ColumnName = "JobDetails", DataType = typeof(string)},
	            new DataColumn() {ColumnName = "JobFlags", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "SubmittedDate", DataType = typeof(DateTime)},
	            new DataColumn() {ColumnName = "SubmittedBy", DataType = typeof(int)},
	            new DataColumn() {ColumnName = "ScheduleRuleType", DataType = typeof(string)},
	            new DataColumn() {ColumnName = "ScheduleRule", DataType = typeof(string)},
	            new DataColumn() {ColumnName = "StopState", DataType = typeof(int)}
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
				taskType, nextRunTime, lastRunTime, jobDetails, jobFlags, submittedDate, submittedBy, scheduleRuleType,
				serializedScheduleRule, StopState.None);
		}
	}
}