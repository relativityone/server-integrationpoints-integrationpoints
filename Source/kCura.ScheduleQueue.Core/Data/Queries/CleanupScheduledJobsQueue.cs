using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class CleanupScheduledJobsQueue : ICommand
    {
        private readonly IQueueDBContext _context;

        public CleanupScheduledJobsQueue(IQueueDBContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.CleanupScheduledJobsQueue, _context.TableName);
            _context.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
        }
    }
}