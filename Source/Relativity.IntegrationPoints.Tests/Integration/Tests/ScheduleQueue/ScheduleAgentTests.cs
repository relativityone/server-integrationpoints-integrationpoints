using System;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[TestFixture]
	public class ScheduleAgentTests : TestsBase
	{
		[Test]
		public void Agent_ShouldCompleteExecution_WhenScheduleQueueIsEmpty()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			Action action = () => sut.Execute();

			// Assert
			action.ShouldNotThrow();
		}

		[Test]
		public void Agent_ShouldPickUpJob_AndRemoveFromTheQueueAfterExecution()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = HelperManager.JobHelper.ScheduleBasicJob();

			var jobsInQueue = new[] {job.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereProcessed(jobsInQueue);
			
			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[Test]
		public void Agent_ShouldProcessTwoJobs_InOneExecutionTrigger()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job1 = HelperManager.JobHelper.ScheduleBasicJob();
			JobTest job2 = HelperManager.JobHelper.ScheduleBasicJob();

			var jobsInQueue = new[] {job1.JobId, job2.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[Test]
		public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedIntegrationPointNotExist()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = HelperManager.JobHelper.ScheduleBasicJob();

			HelperManager.IntegrationPointHelper.RemoveIntegrationPoint(job.RelatedObjectArtifactID);

			var jobsInQueue = new[] { job.JobId };

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[Test]
		public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedWorkspaceNotExist()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = HelperManager.JobHelper.ScheduleBasicJob();

			HelperManager.WorkspaceHelper.RemoveWorkspace(job.WorkspaceID);

			var jobsInQueue = new[] { job.JobId };

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		private FakeAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			return new FakeAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
