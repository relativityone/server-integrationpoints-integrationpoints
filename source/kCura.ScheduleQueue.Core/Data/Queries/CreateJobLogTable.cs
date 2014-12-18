using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CreateJobLogTable
	{
		private IQueueDBContext qDBContext = null;
		public CreateJobLogTable(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.CreateJobLogTable, qDBContext.TableName);
			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
