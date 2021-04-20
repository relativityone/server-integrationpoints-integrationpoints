﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
	public class JobHelper : RelativityHelperBase
	{

		public JobHelper(RelativityInstanceTest relativity) : base(relativity)
		{
		}

		public JobTest ScheduleJob(JobTest job)
		{
			Relativity.JobsInQueue.Add(job);

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
			IntegrationPointTest integrationPoint = workspace.Helpers.IntegrationPointHelper.CreateEmptyIntegrationPoint();
			return CreateBasicJob(workspace, integrationPoint);
		}

		private JobBuilder CreateBasicJob(WorkspaceTest workspace, IntegrationPointTest integrationPoint)
		{
			return new JobBuilder()
				.WithWorkspace(workspace)
				.WithIntegrationPoint(integrationPoint)
				.WithSubmittedBy(Relativity.TestContext.User.ArtifactId);
		}

		#region Verification

		public void VerifyJobsWithIdsAreInQueue(IEnumerable<long> jobs)
		{
			Relativity.JobsInQueue.Select(x => x.JobId).Should().Contain(jobs);
		}

		public void VerifyJobsWithIdsWereRemovedFromQueue(IEnumerable<long> jobs)
		{
			Relativity.JobsInQueue.Select(x => x.JobId).Should().NotContain(jobs);
		}

		public void VerifyJobsAreNotLockedByAgent(AgentTest agent, IEnumerable<long> jobs)
		{
			Relativity.JobsInQueue.Where(x => jobs.Contains(x.JobId))
				.All(x => x.LockedByAgentID != agent.ArtifactId).Should().BeTrue();
		}

		public void VerifyScheduledJobWasReScheduled(JobTest job, DateTime expectedNextRunTime)
		{
			Relativity.JobsInQueue.Should().Contain(x =>
				x.RelatedObjectArtifactID == job.RelatedObjectArtifactID &&
				x.WorkspaceID == job.WorkspaceID &&
				x.NextRunTime.Date == expectedNextRunTime.Date);
		}

		#endregion
	}
}
