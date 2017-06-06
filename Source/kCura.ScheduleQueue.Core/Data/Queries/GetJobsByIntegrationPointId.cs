using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobsByIntegrationPointId
    {
        private readonly IQueueDBContext _qDbContext = null;
        public GetJobsByIntegrationPointId(IQueueDBContext qDbContext)
        {
            _qDbContext = qDbContext;
        }

        public DataTable Execute(long integrationPointId)
        {
            string query = $@"SELECT [JobID]
	  ,[RootJobID]
	  ,[ParentJobID]
	  ,[AgentTypeID]
	  ,[LockedByAgentID]
	  ,[WorkspaceID]
	  ,[RelatedObjectArtifactID]
	  ,[TaskType]
	  ,[NextRunTime]
	  ,[LastRunTime]
	  ,[ScheduleRuleType]
	  ,[ScheduleRule]
	  ,[JobDetails]
	  ,[JobFlags]
	  ,[SubmittedDate]
	  ,[SubmittedBy]
	  ,[StopState] FROM [eddsdbo].[{_qDbContext.TableName}] WHERE RelatedObjectArtifactID = @RelatedObjectArtifactID";

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", integrationPointId));
            return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(query, sqlParams);
        }
    }
}
