using System;
using System.Linq;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Feature.ScheduleQueue
{
	[TestFixture]
	public class SimpleScheduleAgentTests : TestsBase
	{
		[Test]
		public void Agent_ShouldCompleteExecution_WhenScheduleQueueIsEmpty()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

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
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Job job = HelperManager.JobHelper.ScheduleEmptyJob(agent, DateTime.MinValue);

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.ProcessedJobIds.Should().Contain(job.JobId);
			Database.JobsInQueue.Should().NotContain(x => x.JobId == job.JobId);
		}

		[Test]
		public void Agent_ShouldProcessTwoJobs_InOneExecutionTrigger()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Job job1 = HelperManager.JobHelper.ScheduleEmptyJob(agent, DateTime.MinValue);
			Job job2 = HelperManager.JobHelper.ScheduleEmptyJob(agent, DateTime.MinValue);

			Job[] expectedProcessedJobs = new[] {job1, job2};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.ProcessedJobIds.Should().Contain(expectedProcessedJobs.Select(x => x.JobId));
			Database.JobsInQueue.Should().NotContain(expectedProcessedJobs);
		}

		[Test]
		public void Agent_ShouldNotProcess_InvalidJob()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Workspace workspace = HelperManager.WorkspaceHelper.CreateWorkspace("Test Workspace");

			IntegrationPoint integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(workspace);

			Job job = HelperManager.IntegrationPointHelper.ScheduleIntegrationPointJob(integrationPoint);
			
			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.ProcessedJobIds.Should().NotContain(job.JobId);
		}

		private ScheduleTestAgent PrepareSutWithMockedQueryManager(Agent agent)
		{
			return new ScheduleTestAgent(
				agent.AgentGuid,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}

		private Job ScheduleJobWithWorkspaceAndIntegrationPoint(Agent agent, Workspace workspace, IntegrationPoint integrationPoint)
		{
			Job job = new Job
			{
				JobId = JobId.Next,
				AgentTypeID = agent.AgentTypeId,
				NextRunTime = new DateTime(2020, 1, 1),
				JobDetails = string.Empty,
				RelatedObjectArtifactID = integrationPoint.ArtifactId,
				WorkspaceID = workspace.ArtifactId
			};

			Database.JobsInQueue.Add(job);

			return job;
		}
	}
}
