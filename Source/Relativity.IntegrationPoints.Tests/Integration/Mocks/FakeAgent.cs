using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Common.Agent;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeAgent : Agent
	{
		private readonly bool _shouldRunOnce;
		private readonly IWindsorContainer _container;

		public Func<Job, TaskResult> ProcessJobMockFunc { get; set; }
		public List<long> ProcessedJobIds { get; } = new List<long>();

		public FakeAgent(IWindsorContainer container, AgentTest agent, IAgentHelper helper, IAgentService agentService = null, IJobService jobService = null,
				IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null,
				IQueueQueryManager queryManager = null, IAPILog logger = null, bool shouldRunOnce = true)
			: base(agent.AgentGuid, agentService, jobService, scheduleRuleFactory,
				queueJobValidator, queryManager, logger)
		{
			_shouldRunOnce = shouldRunOnce;
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

		public static FakeAgent Create(RelativityInstanceTest instance, IWindsorContainer container, bool shouldRunOnce = true)
		{
			var agent = instance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			var fakeAgent = new FakeAgent(container, agent,
				container.Resolve<IAgentHelper>(),
				queryManager: container.Resolve<IQueueQueryManager>(),
				shouldRunOnce: shouldRunOnce);

			container
				.Register(Component.For<IRemovableAgent>().UsingFactoryMethod(c => fakeAgent)
				.Named(Guid.NewGuid().ToString())
				.IsDefault());

			return fakeAgent;
		}

		public static FakeAgent CreateWithEmptyProcessJob(RelativityInstanceTest instance, IWindsorContainer container, bool shouldRunOnce = true)
		{
			FakeAgent agent = Create(instance, container, shouldRunOnce);
			
			agent.ProcessJobMockFunc = (Job job) => new TaskResult { Status = TaskStatusEnum.Success };

			return agent;
		}

		protected override IEnumerable<int> GetListOfResourceGroupIDs()
		{
			return Const.Agent.RESOURCE_GROUP_IDS;
		}

		protected override IWindsorContainer CreateAgentLevelContainer()
		{
			return _container;
		}

		protected override TaskResult ProcessJob(Job job)
		{
			ProcessedJobIds.Add(job.JobId);

			if (_shouldRunOnce)
			{
				Enabled = false;
			}

			if (ProcessJobMockFunc != null)
			{
				return ProcessJobMockFunc(job);
			}

			_container.Register(Component.For<Job>().UsingFactoryMethod(k => job).Named(Guid.NewGuid().ToString()).IsDefault());

			return base.ProcessJob(job);
		}

		public void MarkAgentToBeRemoved()
		{
			ToBeRemoved = true;
		}

		#region Verification

		public void VerifyJobsWereProcessed(IEnumerable<long> jobs)
		{
			ProcessedJobIds.Should().Contain(jobs);
		}

		public void VerifyJobsWereNotProcessed(IEnumerable<long> jobs)
		{
			ProcessedJobIds.Should().NotContain(jobs);
		}

		public void VerifyJobWasProcessedAtFirst(long jobId)
		{
			ProcessedJobIds.First().Should().Be(jobId);
		}

		#endregion
	}
}
