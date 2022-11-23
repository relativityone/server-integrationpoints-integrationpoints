using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class QueueRepository : IQueueRepository
    {
        private const string _SCHEDULE_AGENT_QUEUE_TABLE_NAME = GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME;
        private readonly IEddsDBContext _dbContext;

        public QueueRepository(IEddsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
        {
            string queuedOrRunningJobsSql =
                $@"SELECT count(*) 
                FROM [{_SCHEDULE_AGENT_QUEUE_TABLE_NAME}] 
                WHERE [WorkspaceID] = @workspaceId 
                    AND [RelatedObjectArtifactID] = @integrationPointId 
                    AND [ScheduleRuleType] is null";

            IEnumerable<SqlParameter> queuedOrRunningJobParameters = new List<SqlParameter>
            {
                new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
                new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
            };

            string scheduledRunningJobsSql = 
            
            $@"SELECT count(*) 
            FROM [{_SCHEDULE_AGENT_QUEUE_TABLE_NAME}] 
            WHERE [WorkspaceID] = @workspaceId 
                AND [RelatedObjectArtifactID] = @integrationPointId 
                AND [ScheduleRuleType] is not null 
                AND [LockedByAgentID] is not null";

            IEnumerable<SqlParameter> scheduledRunningJobParameters = new List<SqlParameter>
            {
                new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
                new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
            };
            
            int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningJobsSql, queuedOrRunningJobParameters);
            numberOfJobs += _dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningJobsSql, scheduledRunningJobParameters);

            return numberOfJobs;
        }

        public int GetNumberOfPendingJobs(int workspaceId, int integrationPointId)
        {
            string pendingJobsSql =

            $@"SELECT count(*) 
            FROM [{_SCHEDULE_AGENT_QUEUE_TABLE_NAME}] 
            WHERE [WorkspaceID] = @workspaceId 
                AND [RelatedObjectArtifactID] = @integrationPointId             
                AND [LockedByAgentID] is null";

            IEnumerable<SqlParameter> queuedOrRunningJobParameters = new List<SqlParameter>
            {
                new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
                new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
            };

            int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(pendingJobsSql, queuedOrRunningJobParameters);

            return numberOfJobs;
        }


        public int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
        {
            string sql = 
                
            $@"SELECT count(*) 
            FROM [{_SCHEDULE_AGENT_QUEUE_TABLE_NAME}] 
            WHERE [WorkspaceID] = @workspaceId 
                AND [RelatedObjectArtifactID] = @integrationPointId 
                AND [LockedByAgentID] is not null 
                AND [NextRunTime] <= @dateValue 
                AND [JobID] != @jobId";

            IEnumerable<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
                new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
                new SqlParameter("@dateValue", SqlDbType.DateTime) {Value = runTime},
                new SqlParameter("@jobId", SqlDbType.BigInt) {Value = jobId}
            };

            int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql, parameters);

            return numberOfJobs;
        }

        public int GetNumberOfJobsLockedByAgentForIntegrationPoint(int workspaceId, int integrationPointId)
        {
            string sql =

            $@"SELECT count(*) 
            FROM [{_SCHEDULE_AGENT_QUEUE_TABLE_NAME}] 
            WHERE [WorkspaceID] = @workspaceId 
                AND [RelatedObjectArtifactID] = @integrationPointId 
                AND [LockedByAgentID] is not null";

            IEnumerable<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
                new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId}
            };

            int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql, parameters);

            return numberOfJobs;
        }
    }
}