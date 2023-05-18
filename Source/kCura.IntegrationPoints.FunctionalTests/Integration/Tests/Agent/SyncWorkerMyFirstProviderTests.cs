using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services.Choice;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [TestExecutionCategory.CI, TestLevel.L1]
    public class SyncWorkerMyFirstProviderTests : TestsBase
    {
        private MyFirstProviderUtil _myFirstProviderUtil;

        public override void SetUp()
        {
            base.SetUp();

            _myFirstProviderUtil = new MyFirstProviderUtil(Container, FakeRelativityInstance,
                SourceWorkspace, Serializer);
        }

        [IdentifiedTest("BCF72894-224F-4DB7-985F-0C53C93D153D")]
        public void SyncWorker_ShouldImportData()
        {
            // Arrange
            const int numberOfRecords = 1000;
            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) => { importJob.Complete(); });

            jobHistory.TotalItems = 2000;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
        }

        [IdentifiedTest("A1350299-3F8E-4215-9773-82EB6185079C")]
        public void SyncWorker_ShouldDrainStop_WhenStopRequestedBeforeIAPI()
        {
            // Arrange
            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();
            agent.ToBeRemoved = true;

            const int numberOfRecords = 1000;
            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) => { throw new Exception("IAPI should not be run"); });

            jobHistory.TotalItems = 2000;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.ItemsTransferred.Should().Be(null);
            jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.DrainStopped);
        }

        [IdentifiedTest("BCF72894-224F-4DB7-985F-0C53C93D153D")]
        public void SyncWorker_ShouldImportData_NotFullBatch()
        {
            // Arrange
            const int numberOfRecords = 420;
            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) => { importJob.Complete(); });

            jobHistory.TotalItems = 2000;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
        }

        [IdentifiedTest("72118579-91DB-4018-8EF9-A4EB3FC2CD51")]
        public void SyncWorker_ShouldDrainStop()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int drainStopAfterImporting = 50;

            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) =>
            {
                importJob.Complete(maxTransferredItems: drainStopAfterImporting);

                agent.ToBeRemoved = true;
            });

            // Act
            sut.Execute(job.AsJob());

            // Assert
            List<string> remainingItems = _myFirstProviderUtil.GetRemainingItems(job);

            remainingItems.Count.Should().Be(numberOfRecords - drainStopAfterImporting);
            remainingItems.Should().BeEquivalentTo(Enumerable
                .Range(drainStopAfterImporting, numberOfRecords - drainStopAfterImporting).Select(x => x.ToString()));

            jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
            jobHistory.ItemsTransferred.Should().Be(drainStopAfterImporting);
            job.StopState.Should().Be(StopState.DrainStopped);
        }

        [IdentifiedTest("72118579-91DB-4018-8EF9-A4EB3FC2CD51")]
        public void SyncWorker_ShouldNotDrainStop_WhenAllItemsInBatchWereProcessedWithItemLevelErrors()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int numberOfErrors = 100;

            _myFirstProviderUtil.SetupWorkspaceDbContextMock_AsNotLastBatch();

            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            jobHistory.TotalItems = 1000;

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) =>
            {
                importJob.Complete(numberOfItemLevelErrors: numberOfErrors);

                agent.ToBeRemoved = true;
            });

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
        }

        [IdentifiedTest("4D867717-3C3D-4763-9E29-63AAAA435885")]
        public void SyncWorker_ShouldNotDrainStopOtherBatches()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int drainStopAfterImporting = 50;
            const int numberOfBatches = 3;

            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJobs(xmlPath, numberOfBatches, RegisterJobContext);
            FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) =>
            {
                importJob.Complete(maxTransferredItems: drainStopAfterImporting);

                agent.ToBeRemoved = true;
            });

            // Act
            var syncManagerJob = job.AsJob();
            sut.Execute(syncManagerJob);

            // Assert
            job.StopState.Should().Be(StopState.DrainStopped);
            FakeRelativityInstance.JobsInQueue.Where(x => x.JobId != job.JobId).All(x => x.StopState == StopState.None)
                .Should().BeTrue();
        }

        [IdentifiedTest("6D4ED0EA-DDAA-442D-AEED-0C7C805A3FB4")]
        public void SyncWorker_ShouldResumeDrainStoppedJob()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int numberOfErrors = 10;
            const int initialTransferredItems = 50;
            const int initialErroredItems = 50;

            FakeJobStatisticsQuery statisticsQuery = Container.Resolve<IJobStatisticsQuery>() as FakeJobStatisticsQuery;
            statisticsQuery.AlreadyFailedItems = initialErroredItems;
            statisticsQuery.AlreadyTransferredItems = initialTransferredItems;

            string xmlPath = _myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);

            jobHistory.JobStatus = new ChoiceRef(new List<Guid> { JobStatusChoices.JobHistorySuspendedGuid });
            jobHistory.ItemsTransferred = initialTransferredItems;
            jobHistory.ItemsWithErrors = initialErroredItems;

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) => { importJob.Complete(maxTransferredItems: numberOfRecords + numberOfErrors, numberOfItemLevelErrors: numberOfErrors); });

            // Act
            var syncManagerJob = job.AsJob();
            sut.Execute(syncManagerJob);

            // Assert
            FakeRelativityInstance.JobsInQueue.Single(x => x.JobId == job.JobId).StopState.Should().Be(StopState.None);
            jobHistory.JobStatus.Guids.First().Should().Be(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);
            jobHistory.ItemsTransferred.Should().Be(initialTransferredItems + numberOfRecords);
            jobHistory.ItemsWithErrors.Should().Be(initialErroredItems + numberOfErrors);

            jobHistory.ShouldHaveCorrectItemsTransferredUpdateHistory(initialTransferredItems, initialTransferredItems + numberOfRecords);
            jobHistory.ShouldHaveCorrectItemsWithErrorsUpdateHistory(initialErroredItems, initialErroredItems + numberOfErrors);
        }

        public static IEnumerable<TestCaseData> FinalJobHistoryStatusTestCases()
        {
            yield return new TestCaseData(
                new[] { new JobTest { StopState = StopState.None, LockedByAgentID = 1 }, new JobTest { StopState = StopState.DrainStopped } },
                    TOTAL_NUMBER_OF_RECORDS,
                    0,
                    false,
                    JobStatusChoices.JobHistoryProcessingGuid,
                    StopState.None
                )
            {
                TestName = "If there are other batches processing, JobHistory should end up in Processing"
            }.WithId("7591EA8C-CF54-482C-B096-C9C4437D3F11");

            yield return new TestCaseData(
                new[] { new JobTest { StopState = StopState.None, LockedByAgentID = 1 }, new JobTest { StopState = StopState.DrainStopped } },
                50,
                0,
                true,
                JobStatusChoices.JobHistoryProcessingGuid,
                StopState.DrainStopped
            )
            {
                TestName = "If there are other batches processing, JobHistory should end up in Processing (DrainStop)"
            }.WithId("1E0AD09D-2DF1-41B4-BC1F-188FB83CCFEA");

            yield return new TestCaseData(
                new[] { new JobTest { StopState = StopState.DrainStopped } },
                TOTAL_NUMBER_OF_RECORDS,
                0,
                false,
                JobStatusChoices.JobHistorySuspendedGuid,
                StopState.None
            )
            {
                TestName = "If other batches are suspended, JobHistory should end up Suspended"
            }.WithId("AB60A230-3B1C-49E1-8F5A-94B7793C24BF");

            yield return new TestCaseData(
                new[] { new JobTest { StopState = StopState.None } },
                TOTAL_NUMBER_OF_RECORDS,
                0,
                false,
                JobStatusChoices.JobHistoryProcessingGuid,
                StopState.None
            )
            {
                TestName = "If other batches are pending, JobHistory should end up Processing"
            }.WithId("FF78BD63-841D-46DD-8762-845DC2110055");

            yield return new TestCaseData(
                new[] { new JobTest { StopState = StopState.None } },
                50,
                0,
                true,
                JobStatusChoices.JobHistorySuspendedGuid,
                StopState.DrainStopped
            )
            {
                TestName = "If other batches are pending, JobHistory should end up Suspended after drain stop"
            }.WithId("326EA29D-E5DB-4A9C-A0CF-8FA71A7EDA3A");
        }

        private const int TOTAL_NUMBER_OF_RECORDS = 100;

        [TestCaseSource(nameof(FinalJobHistoryStatusTestCases))]
        public void SyncWorker_ShouldSetCorrectJobHistoryStatus(JobTest[] otherJobs,
            int transferredItems, int itemLevelErrors, bool drainStopRequested, Guid expectedJobHistoryStatus, StopState expectedStopState)
        {
            // Arrange
            string xmlPath = _myFirstProviderUtil.PrepareRecords(TOTAL_NUMBER_OF_RECORDS);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            jobHistory.TotalItems = 5000;

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

            _myFirstProviderUtil.PrepareOtherJobs(job, jobHistory, otherJobs);

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) =>
            {
                importJob.Complete(transferredItems, itemLevelErrors);

                if (drainStopRequested)
                {
                    agent.ToBeRemoved = true;
                }
            });

            // Act
            Job syncWorkerJob = job.AsJob();
            sut.Execute(syncWorkerJob);

            // Assert
            FakeRelativityInstance.JobsInQueue.First(x => x.JobId == job.JobId).StopState.Should().Be(expectedStopState);
            jobHistory.JobStatus.Guids.First().Should().Be(expectedJobHistoryStatus);
            jobHistory.ItemsTransferred.Should().Be(transferredItems);
            jobHistory.ItemsWithErrors.Should().Be(itemLevelErrors);
        }

        [IdentifiedTest("5C618B8A-D8F5-4BD8-B83A-CC9A289093BF")]
        public void SyncWorker_ShouldMarkJobAsFailedOnIAPIException()
        {
            // Arrange
            string xmlPath = _myFirstProviderUtil.PrepareRecords(TOTAL_NUMBER_OF_RECORDS);
            JobTest job = _myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) => { throw new Exception(); });

            // Act & Assert
            Action act = () => sut.Execute(job.AsJob());

            act.ShouldThrow<Exception>();

            jobHistory.JobStatus.Guids.First().Should().Be(JobStatusChoices.JobHistoryErrorJobFailedGuid);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
        }

        [IdentifiedTest("0D0FC321-0033-4537-8A78-1D8F5B9B598F")]
        public void SyncWorker_ShouldRemoveFailedItemsFromEntityManagerMap()
        {
            // Arrange
            const int numberOfRecords = 100;
            const int numberOfErrors = 60;

            Container.Register(Component.For<IEntityManagerLinksSanitizer>().ImplementedBy<EntityManagerLinksSanitizer>().IsDefault());

            _myFirstProviderUtil.SetupWorkspaceDbContextMock_AsNotLastBatch();
            string xmlPath = _myFirstProviderUtil.PrepareRecordsWithEntities(numberOfRecords);
            JobTest job = _myFirstProviderUtil.PrepareJobWithEntities(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext);
            jobHistory.TotalItems = 1000;

            IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

            SyncWorker sut = _myFirstProviderUtil.PrepareSut((importJob) =>
            {
                importJob.Complete(numberOfItemLevelErrors: numberOfErrors);

                agent.ToBeRemoved = true;
            });

            // Act
            sut.Execute(job.AsJob());

            // Assert
            JobTest createdJob = FakeRelativityInstance.JobsInQueue.Single(x => x.ParentJobId == job.JobId);
            EntityManagerJobParameters jobParameters = createdJob.DeserializeDetails<EntityManagerJobParameters>();
            jobParameters.EntityManagerMap.Should().HaveCount(numberOfRecords - numberOfErrors);
            FakeRelativityInstance.JobsInQueue.TrueForAll(x => x.StopState == StopState.None);
        }
    }
}
