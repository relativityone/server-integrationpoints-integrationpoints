using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.ScheduleQueue.Core.Properties;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class CreateNewAndDeleteOldScheduledJob : ICommand
    {
        private readonly IQueueDBContext _dbContext;
        private readonly long _oldScheduledJobId;
        private readonly int _workspaceId;
        private readonly int _relatedObjectArtifactId;
        private readonly Guid? _correlationId;
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

        public CreateNewAndDeleteOldScheduledJob(
            IQueueDBContext dbContext,
            long oldScheduledJobId,
            int workspaceID,
            int relatedObjectArtifactID,
            Guid? correlationID,
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

            _oldScheduledJobId = oldScheduledJobId;
            _workspaceId = workspaceID;
            _relatedObjectArtifactId = relatedObjectArtifactID;
            _correlationId = correlationID;
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

        public void Execute()
        {
            IEddsDBContext dbContext = _dbContext.EddsDBContext;
            try
            {
                dbContext.BeginTransaction();

                DeleteJob(dbContext);

                CreateScheduledJob(dbContext);

                dbContext.CommitTransaction();
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }

        private void DeleteJob(IEddsDBContext dbContext)
        {
            string sql = string.Format(Resources.DeleteJob, _dbContext.TableName);
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@JobID", _oldScheduledJobId));

            dbContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
        }

        private void CreateScheduledJob(IEddsDBContext dbContext)
        {
            string sql = string.Format(Resources.CreateScheduledJob, _dbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@WorkspaceID", _workspaceId));
            sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", _relatedObjectArtifactId));
            sqlParams.Add(_correlationId == Guid.Empty
                ? new SqlParameter("@CorrelationID", DBNull.Value)
                : new SqlParameter("@CorrelationID", _correlationId));
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

            dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
        }
    }
}
