using System;
using System.Data;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class DatabaseSchema
	{
		public static DataTable AgentSchema()
		{
			var dt = new DataTable();
			dt.Columns.AddRange(new DataColumn[]
			{
				new DataColumn() {ColumnName = "AgentTypeID", DataType = typeof(int)},
				new DataColumn() {ColumnName = "Name", DataType = typeof(string)},
				new DataColumn() {ColumnName = "Fullnamespace", DataType = typeof(string), AllowDBNull = true},
				new DataColumn() {ColumnName = "Guid", DataType = typeof(Guid)}
			});

			return dt;
		}

		public static DataTable ScheduleQueueSchema()
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
	}
}
