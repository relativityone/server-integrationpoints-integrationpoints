using System;
using System.Data;
using System.Text;
using Dapper.Contrib.Extensions;

namespace kCura.IntegrationPoints.Data
{
    [Table("[ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]")]
    public class Job
    {
        public Job()
        {
        }

        public Job(DataRow row)
        {
            JobId = row.Field<long>("JobID");
            RootJobId = row.Field<long?>("RootJobId");
            ParentJobId = row.Field<long?>("ParentJobId");
            AgentTypeID = row.Field<int>("AgentTypeID");
            LockedByAgentID = row.Field<int?>("LockedByAgentID");
            WorkspaceID = row.Field<int>("WorkspaceID");
            RelatedObjectArtifactID = row.Field<int>("RelatedObjectArtifactID");
            CorrelationID = row.Field<string>("CorrelationID");
            TaskType = row.Field<string>("TaskType");
            NextRunTime = row.Field<DateTime>("NextRunTime");
            LastRunTime = row.Field<DateTime?>("LastRunTime");
            JobDetails = row.Field<string>("JobDetails");
            JobFlags = row.Field<int>("JobFlags");
            SubmittedDate = row.Field<DateTime>("SubmittedDate");
            SubmittedBy = row.Field<int>("SubmittedBy");
            ScheduleRuleType = row.Field<string>("ScheduleRuleType");
            ScheduleRule = row.Field<string>("ScheduleRule");
            StopState = (StopState)row.Field<int>("StopState");
            Heartbeat = row.Field<DateTime?>("Heartbeat");
        }

        [Key]
        public long JobId { get; set; }

        public long? RootJobId { get; set; }

        public long? ParentJobId { get; set; }

        public int AgentTypeID { get; set; }

        public int? LockedByAgentID { get; set; }

        public int WorkspaceID { get; set; }

        public int RelatedObjectArtifactID { get; set; }

        public string CorrelationID { get; set; }

        public string TaskType { get; set; }

        public DateTime NextRunTime { get; set; }

        public DateTime? LastRunTime { get; set; }

        public string ScheduleRuleType { get; set; }

        public string ScheduleRule { get; set; }

        public string JobDetails { get; set; }

        public int JobFlags { get; set; }

        public DateTime SubmittedDate { get; set; }

        public int SubmittedBy { get; set; }

        public StopState StopState { get; set; }

        public DateTime? Heartbeat { get; set; }

        [Write(false)]
        public IsJobFailed JobFailed { get; private set; }

        public void MarkJobAsFailed(Exception ex, bool shouldBreakSchedule, bool maximumConsecutiveFailuresReached)
        {
            JobFailed = new IsJobFailed(ex, shouldBreakSchedule, maximumConsecutiveFailuresReached);
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
                ScheduleRule = ScheduleRule,
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
            {
                return true;
            }

            return false;
        }
    }
}
