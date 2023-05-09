using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using static Relativity.IntegrationPoints.Tests.Integration.Const;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [TestExecutionCategory.CI]
    [TestLevel.L1]
    public class SyncManagerTests : TestsBase
    {
        [IdentifiedTest("F0F133E0-0101-4E21-93C5-A6365FD720B3")]
        public void Execute_ShouldAbortGetUnbatchedIDs_WhenDrainStopTimeoutExceeded()
        {
            // Arrange
            string xmlPath = PrepareRecords();

            Guid customProviderId = Guid.NewGuid();

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateCustomProvider(nameof(FakeCustomProvider), customProviderId);

            FakeCustomProvider customProviderImpl = new FakeCustomProvider()
            {
                GetBatchableIdsFunc = () =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    return null;
                }
            };

            Context.InstanceSettings.DrainStopTimeout = TimeSpan.FromSeconds(1);

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);

            SyncManager sut = PrepareSutWithCustomProvider(customProviderImpl, customProviderId);

            // Act
            RunActionWithDrainStop(() => sut.Execute(job.AsJob()));

            // Assert
            VerifyJobHistoryStatus(jobHistory, JobStatusChoices.JobHistorySuspendedGuid);
        }

        [IdentifiedTest("73B8F442-F0DE-437A-BF8A-493FF0FCC5AB")]
        public void Execute_ShouldComplete_WhenGetBatchableIdsFitsInTime()
        {
            // Arrange
            const int numberOfRecords = 1500;
            string xmlPath = PrepareRecords(numberOfRecords);

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);
            SyncManager sut = PrepareSut<SyncManager>();

            int[] batches = SplitNumberIntoBatches(numberOfRecords, sut.BatchSize);
            Context.InstanceSettings.DrainStopTimeout = TimeSpan.FromSeconds(30);

            // Act
            RunActionWithDrainStop(() => sut.Execute(job.AsJob()));

            // Assert
            VerifyCreatedSyncWorkerJobs(batches, StopState.None);
        }

        [IdentifiedTest("541F3112-394B-45BF-AD73-77A610A77D8A")]
        public void Execute_ShouldNotCreateBatchesWithStopStateSuspendingWhenDrainStoppedBeforeBatchTask()
        {
            // Arrange
            const int numberOfRecords = 5500;
            string xmlPath = PrepareRecords(numberOfRecords);

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();
            Container.Register(Component.For<SyncManagerTest>().ImplementedBy<SyncManagerTest>().LifestyleTransient().IsDefault());

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);
            SyncManagerTest sut = PrepareSut<SyncManagerTest>();

            int[] batches = SplitNumberIntoBatches(numberOfRecords, sut.BatchSize);

            sut.BeforeBatchTaskAction = RemoveAgent;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            VerifyCreatedSyncWorkerJobs(batches, StopState.None);
            VerifySyncManagerStopState(StopState.None);
        }

        [IdentifiedTest("4DAFFD61-412F-4EF3-B3F4-7B019D3062A3")]
        public void Execute_ShouldNotCreateBatchesWithStopStateSuspendingWhenDrainStoppedWithinBatchTask()
        {
            // Arrange
            const int numberOfRecords = 5500;
            string xmlPath = PrepareRecords(numberOfRecords);

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();
            Container.Register(Component.For<SyncManagerTest>().ImplementedBy<SyncManagerTest>().LifestyleTransient().IsDefault());

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);
            SyncManagerTest sut = PrepareSut<SyncManagerTest>();

            int[] batches = SplitNumberIntoBatches(numberOfRecords, sut.BatchSize);

            sut.BeforeCreateBatchJobAction = (syncManager) =>
            {
                if (syncManager.BatchJobCount == 2)
                {
                    RemoveAgent();
                }
            };

            // Act
            sut.Execute(job.AsJob());

            // Assert
            VerifyCreatedSyncWorkerJobs(batches, StopState.None);
            VerifySyncManagerStopState(StopState.None);
        }

        [IdentifiedTest("09663B11-23D1-4114-8F4D-097DE47098BB")]
        public void Execute_ShouldFail_WhenGetBatchableIdsThrowException()
        {
            // Arrange
            string xmlPath = PrepareRecords();
            Guid customProviderId = Guid.NewGuid();

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateCustomProvider(nameof(FakeCustomProvider), customProviderId);

            FakeCustomProvider customProviderImpl = new FakeCustomProvider()
            {
                GetBatchableIdsFunc = () =>
                {
                    throw new InvalidOperationException();
                }
            };

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);

            SyncManager sut = PrepareSutWithCustomProvider(customProviderImpl, customProviderId);

            // Act
            sut.Execute(job.AsJob());

            // Assert
            VerifyJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryErrorJobFailedGuid);
        }

        [IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
        public void SyncManager_ShouldSplitJobIntoBatches()
        {
            // Arrange
            const int numberOfRecords = 1500;
            string xmlPath = PrepareRecords(numberOfRecords);

            SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);

            SyncManager sut = PrepareSut<SyncManager>();

            int[] batches = SplitNumberIntoBatches(numberOfRecords, sut.BatchSize);

            // Act
            sut.Execute(job.AsJob());

            // Assert
            VerifyCreatedSyncWorkerJobs(batches, StopState.None);
        }

        private JobTest PrepareJob(SourceProviderTest provider, out JobHistoryTest jobHistory, string xmlPath = null)
        {
            FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider, identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;

            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
            job.TaskType = TaskType.SyncManager.ToString();
            jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            RegisterJobContext(job);

            return job;
        }

        private SyncManager PrepareSutWithCustomProvider(FakeCustomProvider providerImpl, Guid providerId)
        {
            Container.Register(Component.For<IDataSourceProvider>().UsingFactoryMethod(() => providerImpl)
                .Named(providerId.ToString()));

            return PrepareSut<SyncManager>();
        }

        private T PrepareSut<T>() where T : SyncManager
        {
            Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>().Named(Provider._MY_FIRST_PROVIDER));
            T sut = Container.Resolve<T>();
            return sut;
        }

        private void RunActionWithDrainStop(Action action)
        {
            Thread thread = new Thread(() => action());
            thread.Start();
            RemoveAgent();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            thread.Join();
        }

        private string PrepareRecords(int numberOfRecords = 0)
        {
            string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
            string tmpPath = Path.GetTempFileName();
            File.WriteAllText(tmpPath, xml);
            return tmpPath;
        }

        private void VerifyJobHistoryStatus(JobHistoryTest actual, Guid expectedStatus)
        {
            actual.JobStatus.Guids.Single().Should().Be(expectedStatus);
        }

        private void VerifySyncManagerStopState(StopState stopState)
        {
            JobTest syncManager = FakeRelativityInstance.JobsInQueue
                .FirstOrDefault(x => x.TaskType == TaskType.SyncManager.ToString());
            syncManager.StopState.Should().Be(stopState);
        }

        private void VerifyCreatedSyncWorkerJobs(int[] documentsInSyncWorkerJobs, StopState stopState)
        {
            List<JobTest> syncWorkerJobs = FakeRelativityInstance.JobsInQueue.Where(
                x => x.TaskType == TaskType.SyncWorker.ToString()).ToList();
            for (int i = 0; i < documentsInSyncWorkerJobs.Length; ++i)
            {
                AssertNumberOfRecords(syncWorkerJobs[i], documentsInSyncWorkerJobs[i]);
            }

            syncWorkerJobs.Should().HaveCount(documentsInSyncWorkerJobs.Length);
            syncWorkerJobs.Should().OnlyContain(x => x.StopState == stopState);

            FakeRelativityInstance.JobTrackerResourceTables.Single().Value
                .Should().HaveCount(documentsInSyncWorkerJobs.Length);
        }

        private void AssertNumberOfRecords(JobTest job, int numberOfRecords)
        {
            JArray records = JArray.FromObject(JObject.Parse(job.JobDetails)["BatchParameters"]);
            records.Count.Should().Be(numberOfRecords);
        }

        private void RemoveAgent()
        {
            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();
            agent.ToBeRemoved = true;
        }

        private int[] SplitNumberIntoBatches(int number, int batchSize)
        {
            int numberOfElements = Math.DivRem(number, batchSize, out var result) + 1;
            int[] batches = new int[numberOfElements];
            for (int i = 0; i < numberOfElements; i++)
            {
                batches[i] = batchSize;
            }

            batches[numberOfElements - 1] = result;

            return batches;
        }

        private class SyncManagerTest : SyncManager
        {
            public SyncManagerTest(
                ICaseServiceContext caseServiceContext,
                IDataProviderFactory providerFactory,
                IJobManager jobManager,
                IJobService jobService,
                IHelper helper,
                IIntegrationPointService
                integrationPointService,
                ISerializer serializer,
                IGuidService guidService,
                IJobHistoryService jobHistoryService,
                IJobHistoryErrorService jobHistoryErrorService,
                IScheduleRuleFactory scheduleRuleFactory,
                IManagerFactory managerFactory,
                IEnumerable<IBatchStatus> batchStatuses,
                IAgentValidator agentValidator,
                IDiagnosticLog diagnosticLog)
                    : base(
                        caseServiceContext,
                        providerFactory,
                        jobManager,
                        jobService,
                        helper,
                        integrationPointService,
                        serializer,
                        guidService,
                        jobHistoryService,
                        jobHistoryErrorService,
                        scheduleRuleFactory,
                        managerFactory,
                        batchStatuses,
                        agentValidator,
                        diagnosticLog)
            {
            }

            public Action BeforeBatchTaskAction { get; set; }

            public Action<SyncManager> BeforeCreateBatchJobAction { get; set; }

            public override long BatchTask(Job job, IEnumerable<string> batchIDs)
            {
                BeforeBatchTaskAction?.Invoke();
                return base.BatchTask(job, batchIDs);
            }

            public override void CreateBatchJob(Job job, List<string> batchIDs, long batchStartingIndex)
            {
                BeforeCreateBatchJobAction?.Invoke(this);

                base.CreateBatchJob(job, batchIDs, batchStartingIndex);
            }
        }
    }
}
