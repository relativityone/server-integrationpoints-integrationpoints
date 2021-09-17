using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public static class CorrespondingJobDeleteFactory
	{
		private static readonly Guid _agentGuid = new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

		public static ICorrespondingJobDelete Create(IHelper helper)
		{
			IQueueQueryManager queryManager = new QueueQueryManager(helper, _agentGuid);
			IAgentService agentService = new AgentService(helper, queryManager, _agentGuid);
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(queryManager);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, ToggleProvider.Current, helper);
			return new CorrespondingJobDelete(jobService);
		}
	}
}