using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    internal class UpdateHeartbeat : IQuery<int>
    {
        private readonly IQueueDBContext _queueDbContext;
        private readonly long _jobId;
        private readonly DateTime _heartbeatTime;

        public UpdateHeartbeat(IQueueDBContext queueDbContext, long jobId, DateTime heartbeatTime)
        {
            _queueDbContext = queueDbContext;
            _jobId = jobId;
            _heartbeatTime = heartbeatTime;
        }

        public int Execute()
        {
            string sql = string.Format(Resources.UpdateHeartbeat, _queueDbContext.TableName);

            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@JobID", _jobId),
                new SqlParameter("@HeartbeatTime", _heartbeatTime)
            };

            return _queueDbContext.EddsDBContext.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
        }
    }
}
