using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class UnlockJob : ICommand
    {
        private readonly IQueueDBContext _dbContext;
        
        private readonly long _jobId;

        public UnlockJob(IQueueDBContext dbContext, long jobId)
        {
            _dbContext = dbContext;
            
            _jobId = jobId;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.UnlockJob, _dbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@JobID", _jobId)
            };

            _dbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
        }
    }
}