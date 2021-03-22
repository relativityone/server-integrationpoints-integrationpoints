using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class JobHelper : HelperBase
	{
		private const int _userId = 9;

		public JobHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock)
			: base(manager, database, proxyMock)
		{
		}

		public JobTest ScheduleJob(JobTest job)
		{
			Database.JobsInQueue.Add(job);

			return job;
		}

		public JobTest ScheduleBasicJob(WorkspaceTest workspace, DateTime? nextRunTime = null)
		{
			JobTest job = CreateBasicJob(workspace)
				.Build();

			job.NextRunTime = nextRunTime ?? DateTime.UtcNow;

			return ScheduleJob(job);
		}

		public JobTest ScheduleJobWithScheduleRule(WorkspaceTest workspace, ScheduleRuleTest rule)
		{
			JobTest job = CreateBasicJob(workspace)
				.WithScheduleRule(rule)
				.Build();

			return ScheduleJob(job);
		}

		public JobTest ScheduleIntegrationPointRun(WorkspaceTest workspace, IntegrationPointTest integrationPoint)
		{
			JobTest job = CreateBasicJob(workspace, integrationPoint).Build();
			return ScheduleJob(job);
		}
		
		private JobBuilder CreateBasicJob(WorkspaceTest workspace)
		{
			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(workspace);
			return CreateBasicJob(workspace, integrationPoint);
		}

		private JobBuilder CreateBasicJob(WorkspaceTest workspace, IntegrationPointTest integrationPoint)
		{
			return new JobBuilder()
				.WithWorkspace(workspace)
				.WithIntegrationPoint(integrationPoint)
				.WithSubmittedBy(_userId);
		}

		#region Verification

		public void VerifyJobsWithIdsAreInQueue(IEnumerable<long> jobs)
		{
			Database.JobsInQueue.Select(x => x.JobId).Should().Contain(jobs);
		}

		public void VerifyJobsWithIdsWereRemovedFromQueue(IEnumerable<long> jobs)
		{
			Database.JobsInQueue.Select(x => x.JobId).Should().NotContain(jobs);
		}

		public void VerifyJobsAreNotLockedByAgent(AgentTest agent, IEnumerable<long> jobs)
		{
			Database.JobsInQueue.Where(x => jobs.Contains(x.JobId))
				.All(x => x.LockedByAgentID != agent.ArtifactId).Should().BeTrue();
		}

		public void VerifyScheduledJobWasReScheduled(JobTest job, DateTime expectedNextRunTime)
		{
			Database.JobsInQueue.Should().Contain(x =>
				x.RelatedObjectArtifactID == job.RelatedObjectArtifactID &&
				x.WorkspaceID == job.WorkspaceID &&
				x.NextRunTime.Date == expectedNextRunTime.Date);
		}

		#endregion
	}
}
