using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
    public class QueueDBContext : IQueueDBContext
    {
        public QueueDBContext(IHelper dbHelper, string tableNameName)
        {
            this.DBHelper = dbHelper;
            this.TableName = tableNameName;
        }

        public IHelper DBHelper { get; private set; }
        public string TableName { get; private set; }

        public IDBContext EddsDBContext
        {
            get
            {
                return DBHelper.GetDBContext(-1);
            }
        }
    }
}
