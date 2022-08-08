using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.ScheduleQueue.Core.Core;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class UpdateStopState : IQuery<int>
    {
        private readonly IQueueDBContext _qDbContext;
        private readonly IList<long> _jobIds;
        private readonly StopState _state;

        public UpdateStopState(IQueueDBContext qDbContext, IList<long> jobIds, StopState state)
        {
            this._qDbContext = qDbContext;
            
            _jobIds = jobIds;
            _state = state;
        }

        public int Execute()
        {
            string query = string.Format(Resources.UpdateStopState, _qDbContext.TableName, string.Join(",", _jobIds.Distinct()));
            var sqlParams = new List<SqlParameter> { new SqlParameter("@State", (int)_state) };
            return _qDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(query, sqlParams);
        }
    }
}
