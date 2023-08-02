using System;
using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class JobTest
    {
        private string _jobDetails;

        public long JobId { get; set; }

        public long? RootJobId { get; set; }

        public long? ParentJobId { get; set; }

        public int AgentTypeID { get; set; }

        public int? LockedByAgentID { get; set; }

        public int WorkspaceID { get; set; }

        public int RelatedObjectArtifactID { get; set; }

        public Guid CorrelationID { get; set; }

        public string TaskType { get; set; }

        public DateTime NextRunTime { get; set; }

        public DateTime? LastRunTime { get; set; }

        public string ScheduleRuleType { get; set; }

        public string SerializedScheduleRule { get; set; }

        public int JobFlags { get; set; }

        public DateTime SubmittedDate { get; set; }

        public int SubmittedBy { get; set; }

        public StopState StopState { get; set; }

        public DateTime? Heartbeat { get; set; }

        public string JobDetails
        {
            get
            {
                if (string.IsNullOrEmpty(_jobDetails))
                {
                    _jobDetails = JsonConvert.SerializeObject(JobDetailsHelper);
                }

                return _jobDetails;
            }

            set { _jobDetails = value; }
        }

        public TaskParameters JobDetailsHelper { get; }

        public JobTest()
        {
            JobId = Integration.JobId.Next;
            AgentTypeID = Const.Agent.INTEGRATION_POINTS_AGENT_TYPE_ID;
            JobDetailsHelper = new TaskParameters()
            {
                BatchInstance = CorrelationID
            };
        }

        public T DeserializeDetails<T>()
        {
            TaskParameters parameters = JsonConvert.DeserializeObject<TaskParameters>(JobDetails);

            if (parameters.BatchParameters is T result)
            {
                return result;
            }

            if (parameters.BatchParameters is JObject jObject)
            {
                return jObject.ToObject<T>();
            }

            return JsonConvert.DeserializeObject<T>(parameters.BatchParameters.ToString());
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
            row["LockedByAgentID"] = (object)LockedByAgentID ?? DBNull.Value;
            row["WorkspaceID"] = WorkspaceID;
            row["RelatedObjectArtifactID"] = RelatedObjectArtifactID;
            row["CorrelationID"] = CorrelationID;
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
            row["Heartbeat"] = (object)Heartbeat ?? DBNull.Value;

            dt.Rows.Add(row);

            return dt;
        }

        public Job AsJob()
        {
            return new Job(this.AsDataRow());
        }
    }
}
