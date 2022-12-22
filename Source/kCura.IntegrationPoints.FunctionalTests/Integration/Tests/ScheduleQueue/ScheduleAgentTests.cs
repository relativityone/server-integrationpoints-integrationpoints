using System;
using System.Data;
using FluentAssertions;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class ScheduleAgentTests : TestsBase
    {
        public override void SetUp()
        {
            base.SetUp();

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);
        }

        [IdentifiedTest("70482A9F-21E2-42D7-A9F2-2E83013FFF99")]
        public void Agent_ShouldCompleteExecution_WhenScheduleQueueIsEmpty()
        {
            // Arrange
            var sut = PrepareSut();

            // Act
            Action action = () => sut.Execute();

            // Assert
            action.ShouldNotThrow();
        }

        [IdentifiedTest("2D67D0B5-73E6-4D57-B0F9-4AC118A835B3")]
        public void Agent_ShouldPickUpJob_AndRemoveFromTheQueueAfterExecution()
        {
            // Arrange
            JobTest job = PrepareJob();

            var jobsInQueue = new[] { job.JobId };

            var sut = PrepareSut();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        [IdentifiedTest("2D67D0B5-73E6-4D57-B0F9-4AC118A835B3")]
        public void Agent_ShouldPickUpJob_AndCreateJobHistoryIfAbsent()
        {
            // Arrange
            MyFirstProviderUtil myFirstProviderUtil = new MyFirstProviderUtil(Container, FakeRelativityInstance,
                SourceWorkspace, Serializer);

            string xmlPath = myFirstProviderUtil.PrepareRecords(100);

            SourceProviderTest provider =
                SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;

            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            FakeAgent sut = FakeAgent.Create(this.FakeRelativityInstance, this.Container);

            long[] jobsInQueue = { job.JobId };

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        [IdentifiedTest("B3BFE442-1A05-4B4A-89FD-ABB6AC35B60A")]
        public void Agent_ShouldProcessTwoJobs_InOneExecutionTrigger()
        {
            // Arrange
            JobTest job1 = PrepareJob();
            JobTest job2 = PrepareJob();

            var jobsInQueue = new[] { job1.JobId, job2.JobId };

            var sut = PrepareSut();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        [IdentifiedTest("8577B637-7BF7-4B87-B6CC-0AABF9AF0E09")]
        public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedIntegrationPointNotExist()
        {
            // Arrange
            var agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            JobTest job = PrepareJob();

            SourceWorkspace.Helpers.IntegrationPointHelper.RemoveIntegrationPoint(job.RelatedObjectArtifactID);

            var jobsInQueue = new[] { job.JobId };

            var sut = PrepareSut();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereNotProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        [IdentifiedTest("6D71DE00-B990-40C7-AFBB-1F1245A68176")]
        public void Agent_ShouldNotProcessAndDelete_WhenJobRelatedWorkspaceNotExist()
        {
            // Arrange
            JobTest job = PrepareJob();

            FakeRelativityInstance.Helpers.WorkspaceHelper.RemoveWorkspace(job.WorkspaceID);

            var jobsInQueue = new[] { job.JobId };

            var sut = PrepareSut();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereNotProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        [IdentifiedTest("0B9C8AC9-B23F-4920-A124-6295D7E7201B")]
        public void Agent_ShouldUnlockTheJob_WhenUnexpectedExceptionHasBeenThrown()
        {
            // Arrange
            JobTest job = PrepareJob();

            var jobsInQueue = new[] { job.JobId };

            var sut = PrepareSut();

            sut.ProcessJobMockFunc = j => throw new Exception();

            // Act
            sut.Execute();

            // Assert
            FakeRelativityInstance.Helpers.JobHelper
                .VerifyJobsAreNotLockedByAgent(sut.AgentID, jobsInQueue);
        }

        [IdentifiedTest("DED49CD4-5B3A-4FD9-81C9-CBCCD419CDC5")]
        public void Agent_ShouldProcessAndDelete_WhenUserDoesNotExist()
        {
            // Arrange
            JobTest job = PrepareJob();

            FakeRelativityInstance.TestContext.User = null;

            var jobsInQueue = new[] { job.JobId };

            var sut = PrepareSut();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(jobsInQueue);

            FakeRelativityInstance.Helpers.JobHelper.VerifyJobsWithIdsWereRemovedFromQueue(jobsInQueue);
        }

        private FakeAgent PrepareSut()
        {
            return FakeAgent.CreateWithEmptyProcessJob(FakeRelativityInstance, Container, shouldRunOnce: false);
        }

        private JobTest PrepareJob()
        {
            return FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);
        }
    }
}
