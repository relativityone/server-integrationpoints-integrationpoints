using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class CreateScheduledJob : IQuery<DataTable>
    {
        private readonly IQueueDBContext _dbContext;
        private readonly int _workspaceId;
        private readonly int _relatedObjectArtifactId;
        private readonly string _taskType;
        private readonly DateTime _nextRunTime;
        private readonly int _agentTypeId;
        private readonly string _scheduleRuleType;
        private readonly string _serializedScheduleRule;
        private readonly string _jobDetails;
        private readonly int _jobFlags;
        private readonly int _submittedBy;
        private readonly long? _rootJobId;
        private readonly long? _parentJobId;

        public CreateScheduledJob(IQueueDBContext dbContext,
            int workspaceID,
            int relatedObjectArtifactID,
            string taskType,
            DateTime nextRunTime,
            int AgentTypeID,
            string scheduleRuleType,
            string serializedScheduleRule,
            string jobDetails,
            int jobFlags,
            int SubmittedBy,
            long? rootJobID,
            long? parentJobID = null)
        {
            _dbContext = dbContext;

            _workspaceId = workspaceID;
            _relatedObjectArtifactId = relatedObjectArtifactID;
            _taskType = taskType;
            _nextRunTime = nextRunTime;
            _agentTypeId = AgentTypeID;
            _scheduleRuleType = scheduleRuleType;
            _serializedScheduleRule = serializedScheduleRule;
            _jobDetails = jobDetails;
            _jobFlags = jobFlags;
            _submittedBy = SubmittedBy;
            _rootJobId = rootJobID;
            _parentJobId = parentJobID;
        }

        public DataTable Execute()
        {
            string sql = string.Format(Resources.CreateScheduledJob, _dbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@WorkspaceID", _workspaceId));
            sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", _relatedObjectArtifactId));
            sqlParams.Add(new SqlParameter("@TaskType", _taskType));
            sqlParams.Add(new SqlParameter("@NextRunTime", _nextRunTime));
            sqlParams.Add(new SqlParameter("@AgentTypeID", _agentTypeId));
            sqlParams.Add(new SqlParameter("@JobFlags", _jobFlags));
            sqlParams.Add(new SqlParameter("@SubmittedBy", _submittedBy));
            sqlParams.Add(_jobDetails == null
                ? new SqlParameter("@JobDetails", DBNull.Value)
                : new SqlParameter("@JobDetails", _jobDetails));
            sqlParams.Add(string.IsNullOrEmpty(_scheduleRuleType)
                ? new SqlParameter("@ScheduleRuleType", DBNull.Value)
                : new SqlParameter("@ScheduleRuleType", _scheduleRuleType));
            sqlParams.Add(string.IsNullOrEmpty(_serializedScheduleRule)
                ? new SqlParameter("@ScheduleRule", DBNull.Value)
                : new SqlParameter("@ScheduleRule", _serializedScheduleRule));
            sqlParams.Add(!_rootJobId.HasValue || _rootJobId.Value == 0
                ? new SqlParameter("@RootJobID", DBNull.Value)
                : new SqlParameter("@RootJobID", _rootJobId.Value));
            sqlParams.Add(!_parentJobId.HasValue || _parentJobId.Value == 0
                ? new SqlParameter("@ParentJobID", DBNull.Value)
                : new SqlParameter("@ParentJobID", _parentJobId.Value));

            return _dbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
        }
    }
}
