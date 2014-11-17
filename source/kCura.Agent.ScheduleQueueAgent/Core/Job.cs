using System;

namespace kCura.Agent.ScheduleQueueAgent
{
	public class Job
	{
		public long JobId { get; private set; }
		public Int32 AgentTypeID { get; private set; }
		public Int32 Status { get; private set; }
		public Int32? LockedByAgentID { get; private set; }
		public Int32 WorkspaceID { get; private set; }
		public Int32? RelatedObjectArtifactID { get; private set; }
		public string TaskType { get; private set; }
		public DateTime NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public IScheduleRules ScheduleRules { get; set; }
		public string JobDetails { get; set; }
		public Int32 JobFlags { get; set; }
		public DateTime SubmittedDate { get; set; }
		public Int32 SubmittedBy { get; set; }
	}
}
