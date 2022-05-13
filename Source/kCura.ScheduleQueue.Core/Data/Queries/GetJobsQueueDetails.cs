using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobsQueueDetails : IQuery<DataTable>
	{
		private readonly IQueueDBContext _qDbContext;
		private readonly int _agentTypeId;		

		public GetJobsQueueDetails(IQueueDBContext qDbContext, int agentTypeId)
		{
			_qDbContext = qDbContext;
			_agentTypeId = agentTypeId;			
		}

		public DataTable Execute()
		{
			string sql = string.Format(Resources.GetJobsQueueDetails, _qDbContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();			
			sqlParams.Add(new SqlParameter("@AgentTypeID", _agentTypeId));

			return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
		}


	}
}
