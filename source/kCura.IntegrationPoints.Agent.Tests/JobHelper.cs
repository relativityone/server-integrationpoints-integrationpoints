using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tests
{
	public class JobHelper
	{
		public static Job GetJob(
			long JobID,
			long? RootJobID,
			long? ParentJobID,
			int AgentTypeID,
			int LockedByAgentID,
			int WorkspaceID,
			int RelatedObjectArtifactID,
			TaskType TaskType,
			DateTime NextRunTime,
			DateTime? LastRunTime,
			string JobDetails,
			int JobFlags,
			DateTime SubmittedDate,
			int SubmittedBy,
			string ScheduleRuleType,
			string SerializedScheduleRule
			)
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(new DataColumn[]
			{
				new DataColumn( ){ColumnName = "JobID", DataType = typeof(long)},
				new DataColumn( ){ColumnName = "RootJobID", DataType = typeof(long), AllowDBNull = true},
				new DataColumn( ){ColumnName = "ParentJobID", DataType = typeof(long), AllowDBNull = true},
				new DataColumn( ){ColumnName = "AgentTypeID", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "LockedByAgentID", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "WorkspaceID", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "RelatedObjectArtifactID", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "TaskType", DataType = typeof(string)},
				new DataColumn( ){ColumnName = "NextRunTime", DataType = typeof(DateTime)},
				new DataColumn( ){ColumnName = "LastRunTime", DataType = typeof(DateTime), AllowDBNull = true},
				new DataColumn( ){ColumnName = "JobDetails", DataType = typeof(string)},
				new DataColumn( ){ColumnName = "JobFlags", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "SubmittedDate", DataType = typeof(DateTime)},
				new DataColumn( ){ColumnName = "SubmittedBy", DataType = typeof(int)},
				new DataColumn( ){ColumnName = "ScheduleRuleType", DataType = typeof(string)},
				new DataColumn( ){ColumnName = "ScheduleRule", DataType = typeof(string)}
			});
			DataRow row = dt.NewRow();
			row["JobID"] = JobID;
			if (RootJobID.HasValue) row["RootJobID"] = RootJobID;
			else row["RootJobID"] = DBNull.Value;
			if (ParentJobID.HasValue) row["ParentJobID"] = ParentJobID;
			else row["ParentJobID"] = DBNull.Value;
			row["AgentTypeID"] = AgentTypeID;
			row["LockedByAgentID"] = LockedByAgentID;
			row["WorkspaceID"] = WorkspaceID;
			row["RelatedObjectArtifactID"] = RelatedObjectArtifactID;
			row["TaskType"] = TaskType.ToString();
			row["NextRunTime"] = NextRunTime;
			if (LastRunTime.HasValue) row["LastRunTime"] = LastRunTime.Value;
			else row["LastRunTime"] = DBNull.Value;
			row["JobDetails"] = JobDetails;
			row["JobFlags"] = JobFlags;
			row["SubmittedDate"] = SubmittedDate;
			row["SubmittedBy"] = SubmittedBy;
			row["ScheduleRuleType"] = ScheduleRuleType;
			row["ScheduleRule"] = SerializedScheduleRule;
			dt.Rows.Add(row);
			return new Job(row);
		}
	}
}
