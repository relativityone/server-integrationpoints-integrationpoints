using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
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
