﻿using System;
using System.Data;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class JobTest
	{
		public long JobId { get; set; }
		public long? RootJobId { get; set; }
		public long? ParentJobId { get; set; }
		public int AgentTypeID { get; set; }
		public int? LockedByAgentID { get; set; }
		public int WorkspaceID { get; set; }
		public int RelatedObjectArtifactID { get; set; }
		public string TaskType { get; set; }
		public DateTime NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public string ScheduleRuleType { get; set; }
		public string SerializedScheduleRule { get; set; }
		public string JobDetails { get; set; }
		public int JobFlags { get; set; }
		public DateTime SubmittedDate { get; set; }
		public int SubmittedBy { get; set; }
		public StopState StopState { get; set; }

		public TaskParametersTest JobDetailsHelper { get; }

		public JobTest()
		{
			JobId = Integration.JobId.Next;
			AgentTypeID = Const.Agent.INTEGRATION_POINTS_AGENT_TYPE_ID;
			JobDetailsHelper = new TaskParametersTest()
			{
				BatchInstance = Guid.NewGuid()
			};
			JobDetails = JsonConvert.SerializeObject(JobDetailsHelper);
		}

		public DataRow AsDataRow()
		{
			return AsTable().Rows[0];
		}

		public DataTable AsTable()
		{
			DataTable dt = DatabaseSchema.ScheduleQueueSchema();

			DataRow row = dt.NewRow();

			row["JobID"] = JobId;
			row["RootJobID"] = (object)RootJobId ?? DBNull.Value;
			row["ParentJobID"] = (object)ParentJobId ?? DBNull.Value;
			row["AgentTypeID"] = AgentTypeID;
			row["LockedByAgentID"] = DBNull.Value;
			row["WorkspaceID"] = WorkspaceID;
			row["RelatedObjectArtifactID"] = RelatedObjectArtifactID;
			row["TaskType"] = TaskType;
			row["NextRunTime"] = NextRunTime;
			row["LastRunTime"] = (object)LastRunTime ?? DBNull.Value;
			row["JobDetails"] = JobDetails;
			row["JobFlags"] = JobFlags;
			row["SubmittedDate"] = SubmittedDate;
			row["SubmittedBy"] = SubmittedBy;
			row["ScheduleRuleType"] = ScheduleRuleType;
			row["ScheduleRule"] = SerializedScheduleRule;
			row["StopState"] = StopState;

			dt.Rows.Add(row);

			return dt;
		}
	}
}
