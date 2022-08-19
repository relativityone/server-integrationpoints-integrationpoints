using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Sync;
using Relativity.Testing.Identification;

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

            FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

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
            FakeAgent agentMarkedToBeRemoved = PrepareSutWithCustomSyncJob(SyncJobGracefullyDrainStoppedAction);

            agentMarkedToBeRemoved.Execute();

            VerifyJobHasBeenDrainStopped();

            // (2) Act & Assert
            FakeAgent agentAbleToComplete = PrepareSutWithCustomSyncJob((agent, token) => Task.CompletedTask);

            // Fixing container double initialization in Agent.cs revealed an issue in this test.
            // Container in this test is created once, but we create two instances of FakeAgent and register them in container.
            // While running first job, all necessary services are resolved and they get first instance of FakeAgent class (for example ManagerFactory).
            // We set ToBeRemoved = true for this agent to test drain-stop and that's fine.
            // The problem occurs when we create second FakeAgent - we override its registration in container, but it doesn't matter - all classes are already resolved using first instance of an agent,
            // which has ToBeRemoved flag set to True. When we run this job, it immediately drain stops and job is not removed from the queue, ending up in DrainStoppped status again.
            agentMarkedToBeRemoved.ToBeRemoved = false;

            agentAbleToComplete.Execute();

            VerifyJobHasBeenResumed();
        }

        private void ScheduleSyncJob()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();

            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
                .CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);

            FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
        }

        private FakeAgent PrepareSutWithCustomSyncJob(Func<FakeAgent, CompositeCancellationToken, Task> action)
        {
            FakeAgent agent = FakeAgent.Create(FakeRelativityInstance, Container);

            IExtendedFakeSyncOperations syncOperations = Container.Resolve<IExtendedFakeSyncOperations>();

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
            async (agent, token) =>
            {
                agent.MarkAgentToBeRemoved();
                SpinWait.SpinUntil(
                    () => token.IsDrainStopRequested && JobInQueueHasFlag(StopState.DrainStopping),
                    TimeSpan.FromSeconds(_STOP_MANAGER_TIMEOUT)
                );

                await Task.Delay(100);
            };

        private bool JobInQueueHasFlag(StopState stopState) =>
            FakeRelativityInstance.JobsInQueue.All(x => x.StopState.HasFlag(stopState));

        private bool JobHistoryHasStatus(Guid statusGuid) =>
            SourceWorkspace.JobHistory.Single().JobStatus.Guids[0] == statusGuid;
    }
}
