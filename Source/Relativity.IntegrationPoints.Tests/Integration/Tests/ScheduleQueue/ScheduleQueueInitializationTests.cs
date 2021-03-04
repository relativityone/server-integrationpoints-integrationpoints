using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[TestFixture]
	public class ScheduleQueueInitializationTests : TestsBase
	{
		[Test]
		public void ScheduleQueueTable_ShouldBeCreatedOnce_WhenAgentIsRunning()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();
			
			QueryManagerMock queryManagerMock = (QueryManagerMock)Container.Resolve<IQueryManager>();
			
			ScheduleTestAgent sut = new ScheduleTestAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: queryManagerMock);

			// Act
			sut.Execute();

			// Assert
			queryManagerMock.ShouldCreateQueueTable();
		}

		[Test]
		public void Agent_ShouldRemoveJobs_WhenAreNotLockedAndCorrespondingWorkspaceDoesNotExist()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			JobTest jobWithoutWorkspace = HelperManager.JobHelper.ScheduleJob(new JobTest()
			{
				WorkspaceID = Artifact.NextId()
			});


			ScheduleTestAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Enabled = false;

			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(new [] {jobWithoutWorkspace.JobId});

			HelperManager.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(new []{jobWithoutWorkspace.JobId});
		}

		private ScheduleTestAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			return new ScheduleTestAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
