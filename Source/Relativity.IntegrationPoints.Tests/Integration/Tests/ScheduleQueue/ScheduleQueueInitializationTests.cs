﻿using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[IdentifiedTestFixture("E3F6E382-1680-41EA-9DA5-12003FD1988E")]
	public class ScheduleQueueInitializationTests : TestsBase
	{
		[IdentifiedTest("544F7D10-F5FA-4848-AA50-2543FCB22A03")]
		public void ScheduleQueueTable_ShouldBeCreatedOnce_WhenAgentIsRunning()
		{
			// Arrange
			var agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();
			
			QueryManagerMock queryManagerMock = (QueryManagerMock)Container.Resolve<IQueryManager>();
			
			FakeAgent sut = new FakeAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: queryManagerMock);

			// Act
			sut.Execute();

			// Assert
			queryManagerMock.ShouldCreateQueueTable();
		}

		[IdentifiedTest("C4771EA2-4E49-461C-A57C-5EA8E8C0E4D2")]
		public void Agent_ShouldRemoveJobs_WhenAreNotLockedAndCorrespondingWorkspaceDoesNotExist()
		{
			// Arrange
			var agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			JobTest jobWithoutWorkspace = FakeRelativityInstance.Helpers.JobHelper.ScheduleJob(new JobTest()
			{
				WorkspaceID = ArtifactProvider.NextId()
			});


			FakeAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Enabled = false;

			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(new [] {jobWithoutWorkspace.JobId});

			FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(new []{jobWithoutWorkspace.JobId});
		}

		private FakeAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			return new FakeAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
