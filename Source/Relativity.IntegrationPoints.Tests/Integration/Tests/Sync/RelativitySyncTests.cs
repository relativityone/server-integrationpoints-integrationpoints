using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Sync;
using Relativity.Testing.Identification;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Sync
{
	public class RelativitySyncTests : TestsBase
	{
		private const int _STOP_MANAGER_TIMEOUT = 10;

		[IdentifiedTest("1228BB49-8C07-4DAA-818F-1D736BDD8243")]
		public void Agent_ShouldSuccessfullyProcessSyncJob()
		{
			// Arrange
			ScheduleSyncJob();

			var sut = PrepareSutWithMockedQueryManager();

			// Act

			sut.Execute();

			// Assert
			VerifyJobHasBeenCompleted();
		}

		[IdentifiedTest("947DBE0E-032B-4A54-A5C0-1B87361FDF65")]
		public void Agent_ShouldSuccessfullyProcessSyncJob_WhenJobWasDrainStoppedAndResumed()
		{
			// Arrange
			ScheduleSyncJob();

			// (1) Act & Assert
			var agentMarkedToBeRemoved = PrepareSutWithCustomSyncJob(
				SyncJobGracefullyDrainStoppedAction);

			agentMarkedToBeRemoved.Execute();

			VerifyJobHasBeenDrainStopped();

			// (2) Act & Assert
			var agentAbleToComplete = PrepareSutWithCustomSyncJob((agent, token) => Task.CompletedTask);

			agentAbleToComplete.Execute();

			VerifyJobHasBeenResumed();
		}

		private void ScheduleSyncJob()
		{
			WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();

			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
				.CreateSavedSearchIntegrationPoint(destinationWorkspace);

			FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
		}

		private FakeAgent PrepareSutWithMockedQueryManager()
		{
			var agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			var fakeAgent = new FakeAgent(Container, agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());

			Container
				.Register(Component.For<IRemovableAgent>().UsingFactoryMethod(c => fakeAgent)
				.Named(Guid.NewGuid().ToString())
				.IsDefault());

			return fakeAgent;
		}

		private FakeAgent PrepareSutWithCustomSyncJob(Func<FakeAgent, CompositeCancellationToken, Task> action)
		{
			FakeAgent agent = PrepareSutWithMockedQueryManager();
			
			var syncOperations = Container.Resolve<IExtendedFakeSyncOperations>();

			syncOperations.SetupSyncJob(async token => await action(agent, token).ConfigureAwait(false));

			return agent;
		}

		private void VerifyJobHasBeenCompleted()
		{
			FakeRelativityInstance.JobsInQueue.Should().BeEmpty();

			JobHistoryHasStatus(JobStatusChoices.JobHistoryCompletedGuid).Should().BeTrue();
		}

		private void VerifyJobHasBeenDrainStopped()
		{
			JobInQueueHasFlag(StopState.DrainStopped).Should().BeTrue();

			JobHistoryHasStatus(JobStatusChoices.JobHistorySuspendedGuid).Should().BeTrue();
		}

		private void VerifyJobHasBeenResumed()
		{
			SourceWorkspace.SyncConfigurations.Single()
				.Resuming.Should().BeTrue();

			VerifyJobHasBeenCompleted();
		}

		private Func<FakeAgent, CompositeCancellationToken, Task> SyncJobGracefullyDrainStoppedAction =>
			(agent, token) =>
			{
				agent.MarkAgentToBeRemoved();
				SpinWait.SpinUntil(() =>
						token.IsDrainStopRequested && JobInQueueHasFlag(StopState.DrainStopping),
					TimeSpan.FromSeconds(_STOP_MANAGER_TIMEOUT));
				return Task.CompletedTask;
			};

		private bool JobInQueueHasFlag(StopState stopState) => 
			FakeRelativityInstance.JobsInQueue.All(x => x.StopState.HasFlag(stopState));

		private bool JobHistoryHasStatus(Guid statusGuid) =>
			SourceWorkspace.JobHistory.Single().JobStatus.Guids[0] == statusGuid;
	}
}
