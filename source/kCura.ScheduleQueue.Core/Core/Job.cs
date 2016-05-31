using System;
using System.Data;

namespace kCura.ScheduleQueue.Core
{
	public class Job
	{
		public long JobId { get; private set; }
		public long? RootJobId { get; private set; }
		public long? ParentJobId { get; private set; }
		public Int32 AgentTypeID { get; private set; }
		public Int32? LockedByAgentID { get; private set; }
		public Int32 WorkspaceID { get; private set; }
		public Int32 RelatedObjectArtifactID { get; private set; }
		public string TaskType { get; private set; }
		public DateTime NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public string ScheduleRuleType { get; set; }
		public string SerializedScheduleRule { get; set; }
		public string JobDetails { get; set; }
		public Int32 JobFlags { get; set; }
		public DateTime SubmittedDate { get; set; }
		public Int32 SubmittedBy { get; set; }

		public Job(DataRow row)
		{

			JobId = row.Field<long>("JobID");
			RootJobId = row.Field<long?>("RootJobId");
			ParentJobId = row.Field<long?>("ParentJobId");
			AgentTypeID = row.Field<int>("AgentTypeID");
			LockedByAgentID = row.Field<int?>("LockedByAgentID");
			WorkspaceID = row.Field<int>("WorkspaceID");
			RelatedObjectArtifactID = row.Field<int>("RelatedObjectArtifactID");
			TaskType = row.Field<string>("TaskType");
			NextRunTime = row.Field<DateTime>("NextRunTime");
			LastRunTime = row.Field<DateTime?>("LastRunTime");
			JobDetails = row.Field<string>("JobDetails");
			JobFlags = row.Field<int>("JobFlags");
			SubmittedDate = row.Field<DateTime>("SubmittedDate");
			SubmittedBy = row.Field<int>("SubmittedBy");
			ScheduleRuleType = row.Field<string>("ScheduleRuleType");
			SerializedScheduleRule = row.Field<string>("ScheduleRule");
		}

		//Used for internal unit tests only
		internal Job(int workspaceArtifactId, int integrationPointArtifactId, int submittedByArtifactId, int jobId)
		{
			WorkspaceID = workspaceArtifactId;
			RelatedObjectArtifactID = integrationPointArtifactId;
			SubmittedBy = submittedByArtifactId;
			JobId = jobId;
		}
	}
}
