using System;
using System.Data;
using System.Text;

namespace kCura.IntegrationPoints.Data
{
	public class Job
	{
		public long JobId { get; private set; }
		public long? RootJobId { get; private set; }
		public long? ParentJobId { get; private set; }
		public int AgentTypeID { get; private set; }
		public int? LockedByAgentID { get; private set; }
		public int WorkspaceID { get; private set; }
		public int RelatedObjectArtifactID { get; private set; }
		public string TaskType { get; private set; }
		public DateTime NextRunTime { get; set; }
		public DateTime? LastRunTime { get; set; }
		public string ScheduleRuleType { get; set; }
		public string SerializedScheduleRule { get; set; }
		public string JobDetails { get; set; }
		public int JobFlags { get; set; }
		public DateTime SubmittedDate { get; set; }
		public int SubmittedBy { get; set; }
		public StopState StopState { get; set; }
		public DateTime? Heartbeat { get; set; }

		public IsJobFailed JobFailed { get; private set; }

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
			StopState = (StopState)row.Field<int>("StopState");
			Heartbeat = row.Field<DateTime?>("Heartbeat");
		}

		private Job()
		{

		}

		public void MarkJobAsFailed(Exception ex, bool shouldBreakSchedule)
        {
			JobFailed = new IsJobFailed(ex, shouldBreakSchedule);
        }

		/// <summary>
		/// Creates copy of this object without JobDetails
		/// </summary>
		/// <returns></returns>
		public Job RemoveSensitiveData()
		{
			return new Job()
			{
				JobId = JobId,
				RootJobId = RootJobId,
				ParentJobId = ParentJobId,
				AgentTypeID = AgentTypeID,
				LockedByAgentID = LockedByAgentID,
				WorkspaceID = WorkspaceID,
				RelatedObjectArtifactID = RelatedObjectArtifactID,
				TaskType = TaskType,
				NextRunTime = NextRunTime,
				LastRunTime = LastRunTime,
				ScheduleRuleType = ScheduleRuleType,
				SerializedScheduleRule = SerializedScheduleRule,
				JobDetails = JobDetails != null ? "<sensitive_data>" : null,
				JobFlags = JobFlags,
				SubmittedDate = SubmittedDate,
				SubmittedBy = SubmittedBy,
				StopState = StopState,
				Heartbeat = Heartbeat,
			};
		}

		public bool IsBlocked()
        {
			if (StopState != StopState.None && StopState != StopState.DrainStopped && LockedByAgentID == null)
				return true;
			return false;
        }

        public override string ToString()
        {
			try
            {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"JobId: {JobId}");
				sb.AppendLine($"ParentJobId: {ParentJobId}");
				sb.AppendLine($"RootJobId: {RootJobId}");
				sb.AppendLine($"LockedByAgentId: {LockedByAgentID}");
				sb.AppendLine($"WorkspaceId: {WorkspaceID}");
				sb.AppendLine($"IntegrationPointId: {RelatedObjectArtifactID}");
				sb.AppendLine($"StopState: {StopState}");
				sb.AppendLine($"SubmitedBy: {SubmittedBy}");
				sb.AppendLine($"SubmitedDate: {SubmittedDate}");
				sb.AppendLine($"NextRunTime: {NextRunTime}");
				sb.AppendLine($"Heartbeat: {Heartbeat}");

				return sb.ToString();
			}
			catch (Exception)
            {
				return "<stringify_job_failed>";
            }

        }
    }
}