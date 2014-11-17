using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueueAgent.Data
{
	public class QueueServiceBase
	{
		public QueueServiceBase(string queueTableName)
		{
			QueueTableName = queueTableName;
		}

		public string QueueTableName { get; private set; }
	}
}
