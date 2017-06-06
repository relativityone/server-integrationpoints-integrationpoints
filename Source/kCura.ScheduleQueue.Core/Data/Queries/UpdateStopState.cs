using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class UpdateStopState
    {
        private IQueueDBContext _qDbContext;

        public UpdateStopState(IQueueDBContext qDbContext)
        {
            this._qDbContext = qDbContext;
        }

        public int Execute(IList<long> jobIds, StopState state)
        {
            string query = string.Format(Resources.UpdateStopState, _qDbContext.TableName, string.Join(",", jobIds.Distinct()));
            var sqlParams = new List<SqlParameter> { new SqlParameter("@State", (int)state) };
            return _qDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(query, sqlParams);
        }
    }
}
