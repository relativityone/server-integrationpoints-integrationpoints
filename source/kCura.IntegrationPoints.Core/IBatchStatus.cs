using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core
{
	public interface IBatchStatus
	{
		void JobStarted(Job job);
		void JobComplete(Job job);
	}
}
