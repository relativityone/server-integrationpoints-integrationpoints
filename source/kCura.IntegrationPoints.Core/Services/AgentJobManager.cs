using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services
{
	public class AgentJobManager : IJobManager
	{
		public AgentJobManager(IServiceContext context)
		{
			
		}

		public void CreateJob<T>(T jobDetails, TaskType task)
		{
			throw new NotImplementedException();
		}
	}
}
