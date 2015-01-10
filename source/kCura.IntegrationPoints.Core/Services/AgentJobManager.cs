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
		private readonly kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		public AgentJobManager(IEddsServiceContext context, IJobService jobService, kCura.Apps.Common.Utils.Serializers.ISerializer serializer)
		{
			_context = context;
			_jobService = jobService;
			_serializer = serializer;
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule)
		{
			string serializedDetails = _serializer.Serialize(jobDetails);
			_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), rule, serializedDetails, _context.UserID);
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID)
		{
			string serializedDetails = _serializer.Serialize(jobDetails);
			_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID);
		}
	}
}
