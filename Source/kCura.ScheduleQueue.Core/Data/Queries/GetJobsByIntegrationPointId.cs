using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobsByIntegrationPointId : IQuery<DataTable>
    {
        private readonly IQueueDBContext _qDbContext = null;
        private readonly long _integrationPointId;

        public GetJobsByIntegrationPointId(IQueueDBContext qDbContext, long integrationPointId)
        {
            _qDbContext = qDbContext;
            _integrationPointId = integrationPointId;
        }

        public DataTable Execute()
        {
            string query = $@"SELECT [JobID]
                              ,[RootJobID]
                              ,[ParentJobID]
                              ,[AgentTypeID]
                              ,[LockedByAgentID]
                              ,[WorkspaceID]
                              ,[RelatedObjectArtifactID]
                              ,[CorrelationID]
                              ,[TaskType]
                              ,[NextRunTime]
                              ,[LastRunTime]
                              ,[ScheduleRuleType]
                              ,[ScheduleRule]
                              ,[JobDetails]
                              ,[JobFlags]
                              ,[SubmittedDate]
                              ,[SubmittedBy]
                              ,[StopState]
                              ,[Heartbeat]
                            FROM [eddsdbo].[{_qDbContext.TableName}] WHERE RelatedObjectArtifactID = @RelatedObjectArtifactID";

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", _integrationPointId));
            return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(query, sqlParams);
        }
    }
}
