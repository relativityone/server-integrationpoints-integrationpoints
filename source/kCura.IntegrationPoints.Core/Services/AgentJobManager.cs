using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
	public class AgentJobManager : IJobManager
	{
		private readonly IEddsServiceContext _context;
		private readonly IJobService _jobService;
		public AgentJobManager(IEddsServiceContext context, IJobService jobService)
		{
			_context = context;
			_jobService = jobService;
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule)
		{
			_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), rule, string.Empty, _context.UserID);
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID)
		{
			_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, string.Empty,
				_context.UserID);
		}
	}
}
