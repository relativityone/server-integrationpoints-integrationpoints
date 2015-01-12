using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
	public class AgentJobManager : IJobManager
	{
		private readonly IServiceContext _context;
		private readonly IJobService _jobService;
		public AgentJobManager(IServiceContext context, IJobService jobService)
		{
			_context = context;
			_jobService = jobService;
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int integrationPointID, IScheduleRule rule)
		{
			try
			{
				_jobService.CreateJob(_context.WorkspaceID, integrationPointID, task.ToString(), rule, string.Empty, _context.UserID);
			}
			catch (AgentNotFoundException anfe)
			{
				throw new Exception(Properties.ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int integrationPointID)
		{
			try
			{
				_jobService.CreateJob(_context.WorkspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, string.Empty,
					_context.UserID);
			}
			catch (AgentNotFoundException anfe)
			{
				throw new Exception(Properties.ErrorMessages.NoAgentInstalled, anfe);
			}
		}
	}
}
