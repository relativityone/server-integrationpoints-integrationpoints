using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class UpdateJobDetails : ICommand
    {
        private readonly IQueueDBContext _qDbContext;
        private readonly long _jobId;
        private readonly string _jobDetails;

        public UpdateJobDetails(IQueueDBContext qDbContext, long jobId, string jobDetails)
        {
            _qDbContext = qDbContext;
            _jobId = jobId;
            _jobDetails = jobDetails;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.UpdateJobDetails, _qDbContext.TableName);
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@JobID", _jobId),
                new SqlParameter("@JobDetails", _jobDetails)
            };

            _qDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
        }
    }
}
