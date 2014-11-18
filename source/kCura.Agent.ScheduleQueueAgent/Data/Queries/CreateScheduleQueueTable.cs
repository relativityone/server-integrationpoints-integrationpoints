using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Agent.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class CreateScheduleQueueTable
	{
		private IQueueDBContext qDBContext = null;
		public CreateScheduleQueueTable(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.CreateQueueTable, qDBContext.QueueTable);
			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
