using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.ScheduleQueueAgent;
using kCura.ScheduleQueueAgent.BatchProcess;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker :  ITask
	{
		public SyncWorker()
		{

		}
		
		public void Execute(Job job)
		{
			throw new NotImplementedException();
		}
	}
}
