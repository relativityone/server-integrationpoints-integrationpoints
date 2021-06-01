using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Job = kCura.ScheduleQueue.Core.Job;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeScheduleAgent : ScheduleQueueAgentBase
	{
		public Func<Job, TaskResult> ProcessJobMockFunc { get; set; }

		public List<long> ProcessedJobIds { get; } = new List<long>();

		public FakeScheduleAgent(AgentTest agent, IAgentHelper helper, IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null, 
			IQueryManager queryManager = null, IAPILog logger = null) 
			: base(agent.AgentGuid, agentService, jobService, scheduleRuleFactory, 
				queueJobValidator, queryManager, logger)
		{
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
		
		protected override TaskResult ProcessJob(Job job)
		{
			ProcessedJobIds.Add(job.JobId);

			return ProcessJobMockFunc != null
				? ProcessJobMockFunc(job)
				: new TaskResult { Status = TaskStatusEnum.Success };
		}

		protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
		{
			//Intentionally empty
		}

		protected override IEnumerable<int> GetListOfResourceGroupIDs()
		{
			return Const.Agent.RESOURCE_GROUP_IDS;
		}

		public override string Name { get; }

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
