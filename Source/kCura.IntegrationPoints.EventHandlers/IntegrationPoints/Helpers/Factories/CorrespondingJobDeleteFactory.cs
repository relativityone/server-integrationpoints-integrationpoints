using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

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
            IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IKubernetesMode kubernetesMode = new KubernetesMode(logger);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, kubernetesMode, helper);
			return new CorrespondingJobDelete(jobService);
		}
	}
}