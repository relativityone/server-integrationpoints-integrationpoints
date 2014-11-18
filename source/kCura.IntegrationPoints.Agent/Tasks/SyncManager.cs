using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Agent.ScheduleQueueAgent;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncManager : ITask
	{
		public SyncManager(IDataProviderFactory providerFactory)
		{
			
		}

		public void Execute(Job job)
		{
		}
	}
}
