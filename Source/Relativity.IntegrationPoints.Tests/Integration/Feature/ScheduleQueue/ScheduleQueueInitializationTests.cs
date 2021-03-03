using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Feature.ScheduleQueue
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

			Job jobWithoutWorkspace = HelperManager.JobHelper.ScheduleJob(new Job()
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

		private ScheduleTestAgent PrepareSutWithMockedQueryManager(Agent agent)
		{
			return new ScheduleTestAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
