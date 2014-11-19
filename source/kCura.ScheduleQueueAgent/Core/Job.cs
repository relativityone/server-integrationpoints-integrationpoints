using System;
using System.Data;

namespace kCura.ScheduleQueueAgent
{
	public class Job
	{
		public long JobId { get; private set; }
		public Int32 AgentTypeID { get; private set; }
		public Int32? LockedByAgentID { get; private set; }
		public Int32 WorkspaceID { get; private set; }
		public Int32? RelatedObjectArtifactID { get; private set; }
		public string TaskType { get; private set; }
		public DateTime NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public IScheduleRule ScheduleRule { get; set; }
		public string JobDetails { get; set; }
		public Int32 JobFlags { get; set; }
		public DateTime SubmittedDate { get; set; }
		public Int32 SubmittedBy { get; set; }

		public Job(DataRow row)
		{

			JobId = row.Field<int>("AgentID");
			AgentTypeID = row.Field<int>("AgentTypeID");
			LockedByAgentID = row.Field<int?>("LockedByAgentID");
			WorkspaceID = row.Field<int>("WorkspaceID");
			RelatedObjectArtifactID = row.Field<int?>("RelatedObjectArtifactID");
			TaskType = row.Field<string>("TaskType");
			NextRunTime = row.Field<DateTime>("NextRunTime");
			LastRunTime = row.Field<DateTime?>("LastRunTime");
			JobDetails = row.Field<string>("JobDetails");
			JobFlags = row.Field<int>("JobFlags");
			SubmittedDate = row.Field<DateTime>("SubmittedDate");
			SubmittedBy = row.Field<int>("SubmittedBy");

			//TODO: implement schedule rule instantiation
			string scheduleRules = row.Field<string>("ScheduleRule");
			ScheduleRule = null;
		}
	}
}
