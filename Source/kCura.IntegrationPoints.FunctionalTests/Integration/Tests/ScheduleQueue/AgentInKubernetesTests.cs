using System;
using System.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
    public class AgentInKubernetesTests : TestsBase
    {
        [SetUp]
        public void Setup()
        {
            FakeKubernetesMode kubernetesMode = (FakeKubernetesMode)Container.Resolve<IKubernetesMode>();
            kubernetesMode.SetIsEnabled(true);

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);
        }

        [Test]
        public void Agent_ShouldNotCallGetListOfResourceGroupIDs_WhenKubernetesModeIsEnabled()
        {
            // Arrange
            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

            FakeAgent sut = FakeAgent.CreateWithEmptyProcessJob(FakeRelativityInstance, Container, shouldRunOnce: true);

            sut.GetResourceGroupIDsMockFunc = () => throw new NotSupportedException();

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(new long[] { job.JobId });
        }

        [Test]
        public void Agent_ShouldExecuteJobsInTheLoop_WhenTheRootJobIdMatch()
        {
            // Arrange
            long? rootJobId = ArtifactProvider.NextId();

            IntegrationPointFake integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateEmptyIntegrationPoint();

            JobTest jobWithSameRootId1 = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, null, rootJobId);

            JobTest jobWithoutRootId = FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            JobTest jobWithSameRootId2 = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, null, rootJobId);
            JobTest jobWithSameRootId3 = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, null, rootJobId);

            FakeAgent sut = FakeAgent.CreateWithEmptyProcessJob(FakeRelativityInstance, Container, shouldRunOnce: false);

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(new long[] {
                jobWithSameRootId1.JobId,
                jobWithSameRootId2.JobId,
                jobWithSameRootId3.JobId });

            sut.VerifyJobsWereNotProcessed(new long[] { jobWithoutRootId.JobId });
        }

        [Test]
        public void Agent_ShouldFinishExecution_WhenMaximumLifeTimeWillBeReached()
        {
            // Arrange
            const int jobDurationInHours = 4;

            DateTime utcNow = new DateTime(2000, 1, 1);

            Context.SetDateTime(utcNow);

            long? rootJobId = ArtifactProvider.NextId();

            IntegrationPointFake integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateEmptyIntegrationPoint();

            JobTest jobWithSameRootId1 = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, null, rootJobId);
            JobTest jobWithSameRootId2 = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, null, rootJobId);

            FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, shouldRunOnce: true);

            sut.ProcessJobMockFunc = (job) =>
            {
                Context.SetDateTime(utcNow.Add(TimeSpan.FromHours(jobDurationInHours)));

                return new TaskResult { Status = TaskStatusEnum.Success };
            };

            // Act
            sut.Execute();

            // Assert
            sut.VerifyJobsWereProcessed(new long[]
            {
                jobWithSameRootId1.JobId,
            });

            sut.VerifyJobsWereNotProcessed(new long[]
            {
                jobWithSameRootId2.JobId
            });
        }
    }
}
