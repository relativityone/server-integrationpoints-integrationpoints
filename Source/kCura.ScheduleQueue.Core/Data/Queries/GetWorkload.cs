using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetWorkload : IQuery<int>
    {
        private readonly IQueueDBContext _qDbContext;

        public GetWorkload(IQueueDBContext qDbContext)
        {
            _qDbContext = qDbContext;
        }
        
        public int Execute()
        {
            string sql = string.Format(Resources.GetWorkload, _qDbContext.TableName);
            return _qDbContext.EddsDBContext.ExecuteSqlStatementAsScalar<int>(sql);
        }
    }
}