using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class CorrespondingJobDeleteFactory
	{
		public static ICorrespondingJobDelete Create(IHelper helper)
		{
			IAgentService agentService = new AgentService(helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobService jobService = new JobService(agentService, helper);
			return new CorrespondingJobDelete(jobService);
		}
	}
}