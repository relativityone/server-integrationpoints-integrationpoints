using Castle.Windsor;
using kCura.IntegrationPoints.Agent;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeAgent : Agent
	{
		private readonly IWindsorContainer _container;

		public FakeAgent(IWindsorContainer container, AgentTest agent, IAgentHelper helper, IAgentService agentService = null, IJobService jobService = null,
				IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null,
				IQueryManager queryManager = null, IAPILog logger = null)
			: base(agent.AgentGuid, agentService, jobService, scheduleRuleFactory,
				queueJobValidator, queryManager, logger)
		{
			_container = container;

			//Agent ID setter is marked as private
			typeof(kCura.Agent.AgentBase).GetField("_agentID", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(this, agent.ArtifactId);

			//IAgentHelper setter is marked as private
			typeof(kCura.Agent.AgentBase).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(this, helper);

			//'Enabled = true' triggered Execute() immediately. I needed to set the field only to enable getting job from the queue
			typeof(kCura.Agent.AgentBase).GetField("_enabled", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(this, true);
		}

		protected override IEnumerable<int> GetListOfResourceGroupIDs()
		{
			return Const.Agent.RESOURCE_GROUP_IDS;
		}

		protected override IWindsorContainer CreateAgentLevelContainer()
		{
			return _container;
		}

		public void MarkAgentToBeRemoved()
		{
			ToBeRemoved = true;
		}
	}
}
