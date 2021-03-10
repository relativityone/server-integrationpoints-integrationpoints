﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class JobHelper : HelperBase
	{
		public JobHelper(HelperManager manager, InMemoryDatabase database, ProxyMock proxyMock) : base(manager, database, proxyMock)
		{
		}

		public JobTest ScheduleJob(JobTest job)
		{
			Database.JobsInQueue.Add(job);

			return job;
		}

		public JobTest ScheduleBasicJob(DateTime? nextRunTime = null)
		{
			var job = CreateBasicJob()
				.Build();

			job.NextRunTime = nextRunTime ?? DateTime.UtcNow;

			return ScheduleJob(job);
		}

		public JobTest ScheduleJobWithScheduleRule(ScheduleRuleTest rule)
		{
			JobTest job = CreateBasicJob()
				.WithScheduleRule(rule)
				.Build();

			return ScheduleJob(job);
		}

		private JobBuilder CreateBasicJob()
		{
			WorkspaceTest workspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(workspace);

			return new JobBuilder()
				.WithWorkspace(workspace)
				.WithIntegrationPoint(integrationPoint);
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