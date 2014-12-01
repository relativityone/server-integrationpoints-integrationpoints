using System;

namespace kCura.ScheduleQueueAgent.Logging
{
	public class JobLog
	{
		public string TaskType { get; set; }
		public long JobID { get; set; }
		public JobLogState State { get; set; }
		public Int32? AgentID { get; set; }
		public Int32? RelatedObjectArtifactID { get; set; }
		public Int32 CreatedBy { get; set; }
		public String Details { get; set; }
	}
}
