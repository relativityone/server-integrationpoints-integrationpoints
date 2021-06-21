using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	public class DrainStoppedJobsInScheduleQueueTests : TestsBase
	{
		[IdentifiedTest("6AC120D6-EDBE-4905-8C4B-CDC1340E87F3")]
		public void Agent_ShouldNotPickUpTheJob_WhenHasBeenMarkedToBeRemoved()
		{
			// Arrange
			AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			JobTest job = PrepareJob();

			var jobsInQueue = new[] {job.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.MarkAgentToBeRemoved();

			sut.Execute();

			// Assert
			sut.VerifyJobsWereNotProcessed(jobsInQueue);

			FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsAreInQueue(jobsInQueue);
		}

		[IdentifiedTest("5FD8409E-F0D6-4CE1-88D0-B9601314551B")]
		public void Agent_ShouldNotPickupNextJob_WhenActuallyJobWasDrainStopped()
		{
			// Arrange
			AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			JobTest job1 = PrepareJob();
			JobTest job2 = PrepareJob();

			var jobsInQueue = new[] {job1.JobId, job2.JobId};

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.ProcessJobMockFunc = _ => new TaskResult() {Status = TaskStatusEnum.DrainStopped};

			sut.Execute();

			// Arrange
			sut.VerifyJobsWereProcessed(new[] {job1.JobId});

			sut.VerifyJobsWereNotProcessed(new[] {job2.JobId});

			FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsAreInQueue(jobsInQueue);

			FakeRelativityInstance.Helpers.JobHelper.VerifyJobsAreNotLockedByAgent(agent, jobsInQueue);
		}

		[IdentifiedTest("A21344C7-1CB6-439B-8478-B346B702CD3A")]
		public void Agent_ShouldPickUpDrainStoppedJobAtFirst()
		{
			// Arrange
			AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			JobTest job1 = PrepareJob();
			JobTest job2 = FakeRelativityInstance.Helpers.JobHelper.ScheduleJob(new JobTest()
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

		private FakeScheduleAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			return new FakeScheduleAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}

		private JobTest PrepareJob()
		{
			return FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);
		}
	}
}
