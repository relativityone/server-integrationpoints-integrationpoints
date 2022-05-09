using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class GetJobsQueueDetails : IQuery<DataTable>
	{
		private readonly IQueueDBContext _qDbContext;

		public GetJobsQueueDetails(IQueueDBContext qDbContext)
		{
			_qDbContext = qDbContext;
		}

		public DataTable Execute()
		{
			string sql = string.Format(Resources.GetJobsQueueDetails, _qDbContext.TableName);
			return _qDbContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql);
		}


	}
}
