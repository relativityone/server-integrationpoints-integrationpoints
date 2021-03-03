using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Job = Relativity.IntegrationPoints.Tests.Integration.Models.Job;

namespace Relativity.IntegrationPoints.Tests.Integration.Feature.ScheduleQueue
{
	[TestFixture]
	public class DrainStoppedJobsInScheduleQueueTests : TestsBase
	{
		[Test]
		public void Agent_ShouldNotPickUpTheJob_WhenHasBeenMarkedToBeRemoved()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Job job = HelperManager.JobHelper.ScheduleBasicJob();

			var jobsInQueue = new[] {job.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.MarkAgentToBeRemoved();

			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsWithIdsAreInQueue(jobsInQueue);
		}

		[Test]
		public void Agent_ShouldNotPickupNextJob_WhenActuallyJobWasDrainStopped()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Job job1 = HelperManager.JobHelper.ScheduleBasicJob();
			Job job2 = HelperManager.JobHelper.ScheduleBasicJob();

			var jobsInQueue = new[] {job1.JobId, job2.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.ProcessJobMockFunc = _ => new TaskResult() {Status = TaskStatusEnum.DrainStopped};

			sut.Execute();

			// Arrange
			sut.VerifyJobsWereProcessed(new[] {job1.JobId});

			sut.VerifyJobsWereNotProcessed(new[] {job2.JobId});

			HelperManager.JobHelper.VerifyJobsWithIdsAreInQueue(jobsInQueue);

			HelperManager.JobHelper.VerifyJobsAreNotLockedByAgent(agent, jobsInQueue);
		}

		[Test]
		public void Agent_ShouldPickUpDrainStoppedJobAtFirst()
		{
			// Arrange
			Agent agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			Job job1 = HelperManager.JobHelper.ScheduleBasicJob();
			Job job2 = HelperManager.JobHelper.ScheduleJob(new Job()
			{
				WorkspaceID = job1.WorkspaceID,
				RelatedObjectArtifactID = job1.RelatedObjectArtifactID,
				StopState = StopState.DrainStopped
			});

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Arrange
			sut.VerifyJobWasProcessedAtFirst(job2.JobId);
		}

		private ScheduleTestAgent PrepareSutWithMockedQueryManager(Agent agent)
		{
			return new ScheduleTestAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
