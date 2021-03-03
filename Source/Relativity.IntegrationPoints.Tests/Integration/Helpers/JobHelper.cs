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

		public Job ScheduleJob(Job job)
		{
			Database.JobsInQueue.Add(job);

			return job;
		}

		public Job ScheduleBasicJob()
		{
			var job = CreateBasicJob()
				.Build();

			return ScheduleJob(job);
		}

		public Job ScheduleJobWithSchedule(ScheduleRule rule)
		{
			Job job = CreateBasicJob()
				.WithScheduleRule(rule)
				.Build();

			return ScheduleJob(job);
		}

		private JobBuilder CreateBasicJob()
		{
			Workspace workspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			IntegrationPoint integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(workspace);

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

		public void VerifyJobsAreNotLockedByAgent(Agent agent, IEnumerable<long> jobs)
		{
			Database.JobsInQueue.Where(x => jobs.Contains(x.JobId))
				.All(x => x.LockedByAgentID != agent.ArtifactId).Should().BeTrue();
		}

		#endregion
	}
}
