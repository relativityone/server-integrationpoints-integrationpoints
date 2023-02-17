using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class CreateScheduleQueueTable : ICommand
    {
        private readonly IQueueDBContext qDBContext;

        public CreateScheduleQueueTable(IQueueDBContext qDBContext)
        {
            this.qDBContext = qDBContext;
        }

        public void Execute()
        {
            string sql = string.Format(Resources.CreateQueueTable, qDBContext.TableName);
            qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
        }
    }
}
