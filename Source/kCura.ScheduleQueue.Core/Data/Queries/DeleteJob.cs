using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class DeleteJob : ICommand
    {
        private readonly IEddsDBContext _dbContext;
        private readonly string _tableName;
        private readonly long _jobId;

        public DeleteJob(IQueueDBContext dbContext, long jobId)
        {
            _dbContext = dbContext.EddsDBContext;

            _tableName = dbContext.TableName;
            _jobId = jobId;
        }

        public DeleteJob(IEddsDBContext dbContext, string tableName, long jobId)
        {
            _dbContext = dbContext;
            _tableName = tableName;

            _jobId = jobId;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.DeleteJob, _tableName);
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@JobID", _jobId));

            _dbContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
        }
    }
}
