﻿using System;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[IdentifiedTestFixture("F3B9D08E-5326-42B5-A360-9D874D6D05C6")]
	public class ScheduleAgentTests : TestsBase
	{
		[IdentifiedTest("70482A9F-21E2-42D7-A9F2-2E83013FFF99")]
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

		[IdentifiedTest("2D67D0B5-73E6-4D57-B0F9-4AC118A835B3")]
		public void Agent_ShouldPickUpJob_AndRemoveFromTheQueueAfterExecution()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = PrepareJob();

			var jobsInQueue = new[] {job.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereProcessed(jobsInQueue);
			
			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[IdentifiedTest("B3BFE442-1A05-4B4A-89FD-ABB6AC35B60A")]
		public void Agent_ShouldProcessTwoJobs_InOneExecutionTrigger()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job1 = PrepareJob();
			JobTest job2 = PrepareJob();

			var jobsInQueue = new[] {job1.JobId, job2.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[IdentifiedTest("8577B637-7BF7-4B87-B6CC-0AABF9AF0E09")]
		public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedIntegrationPointNotExist()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = PrepareJob();

			HelperManager.IntegrationPointHelper.RemoveIntegrationPoint(job.RelatedObjectArtifactID);

			var jobsInQueue = new[] { job.JobId };

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
		}

		[IdentifiedTest("6D71DE00-B990-40C7-AFBB-1F1245A68176")]
		public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedWorkspaceNotExist()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = PrepareJob();

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

		private JobTest PrepareJob()
		{
			return HelperManager.JobHelper.ScheduleBasicJob(SourceWorkspace);
		}
	}
}
