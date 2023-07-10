using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Tests.Common.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using FileInfo = System.IO.FileInfo;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.CustomProvider
{
    [TestExecutionCategory.CI]
    [TestLevel.L2]
    public class CustomProviderTests : TestsBase
    {
        private readonly ManagementTestData _managementTestData = new ManagementTestData();

        private ImportState[] ImportStates { get; set; }

        public override void SetUp()
        {
            base.SetUp();

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);

            Context.ToggleValues.SetValue<EnableImportApiV2ForCustomProvidersToggle>(true);

            Context.InstanceSettings.CustomProviderBatchSize = 2;
            Context.InstanceSettings.CustomProviderProgressUpdateInterval = TimeSpan.FromSeconds(100);

            ImportStates = new[]
            {
                ImportState.New, ImportState.Configured, ImportState.Scheduled, ImportState.Idle, ImportState.Completed
            };

            QueueQueryManagerMock.JobDetailsUpdateExecutions = new List<KeyValuePair<long, string>>();

            SetupWaitForJobToFinish();
        }

        [Test]
        public void CustomProviderTest_SystemTest()
        {
            // Arrange
            var job = ScheduleImportCustomProviderJob();

            FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

            // Act
            sut.Execute();

            // Assert
            CustomProviderJobDetails jobDetails = GetJobDetails(job.JobDetails);
            VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedGuid);
            VerifyFileExistenceAndContent(job, jobDetails);
            VerifyTotalItems(jobDetails);
            VerifyImportJobControllerExecutions(job, jobDetails);
            VerifyAddToImportQueue(job, jobDetails);

            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
        }

        private void SetupWaitForJobToFinish()
        {
            var jobProgressHandler = Container.Resolve<IJobProgressHandler>();
            jobProgressHandler.GetType().GetField("WaitForJobToFinishInterval", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(jobProgressHandler, TimeSpan.FromMilliseconds(200));
            Container.Register(Component.For<IJobProgressHandler>().UsingFactoryMethod(() => jobProgressHandler)
                .LifestyleTransient().IsDefault());

            var callIdx = 0;
            var importJobDetails = new ValueResponse<ImportDetails>(
                Guid.Empty,
                true,
                string.Empty,
                string.Empty,
                new ImportDetails(
                    ImportState.Unknown,
                    "Rip and SFU",
                    Context.User.ArtifactId,
                    DateTime.Today,
                    Context.User.ArtifactId,
                    DateTime.UtcNow));

            Proxy.ImportJobController.Mock.Setup(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Callback((int workspaceId, Guid importJobId) =>
                {
                    importJobDetails = new ValueResponse<ImportDetails>(
                        importJobId,
                        true,
                        string.Empty,
                        string.Empty,
                        new ImportDetails(
                            ImportStates[callIdx],
                            kCura.IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_NAME,
                            Context.User.ArtifactId,
                            DateTime.Today,
                            Context.User.ArtifactId,
                            DateTime.UtcNow));
                    callIdx++;
                })
                .Returns((int workspaceId, Guid importJobId) => Task.FromResult(
                    importJobDetails));

            Proxy.ImportSourceControllerStub.Mock.Setup(
                x => x.GetDetailsAsync(SourceWorkspace.ArtifactId, It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns((int workspaceId, Guid importJobId, Guid sourceId) =>
                    Task.FromResult(new ValueResponse<DataSourceDetails>(
                        importJobId,
                        true,
                        string.Empty,
                        string.Empty,
                        new DataSourceDetails
                        {
                            State = DataSourceState.Completed
                        })));
        }

        private void VerifyTotalItems(CustomProviderJobDetails jobDetails)
        {
            Proxy.ObjectManager.Mock.Verify(
                x => x.UpdateAsync(
                    SourceWorkspace.ArtifactId,
                    It.Is<UpdateRequest>(
                        y =>
                            y.Object.ArtifactID == jobDetails.JobHistoryID &&
                            y.FieldValues.FirstOrDefault().Field.Guid == JobHistoryFieldGuids.TotalItemsGuid &&
                            (int)y.FieldValues.FirstOrDefault().Value == _managementTestData.Data.Count)),
                Times.Once);
        }

        private void VerifyImportJobControllerExecutions(JobTest job, CustomProviderJobDetails jobDetails)
        {
            Proxy.ImportJobController.Mock.Verify(
                x => x.CreateAsync(
                    SourceWorkspace.ArtifactId,
                    jobDetails.JobHistoryGuid,
                    kCura.IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_NAME,
                    job.JobId.ToString()),
                Times.Once);
            Proxy.ImportJobController.Mock.Verify(
                x => x.BeginAsync(SourceWorkspace.ArtifactId, jobDetails.JobHistoryGuid),
                Times.Once);
            Proxy.ImportJobController.Mock.Verify(
                x => x.GetProgressAsync(SourceWorkspace.ArtifactId, jobDetails.JobHistoryGuid),
                Times.Exactly(2));
            Proxy.ImportJobController.Mock.Verify(
                x => x.GetDetailsAsync(SourceWorkspace.ArtifactId, jobDetails.JobHistoryGuid),
                Times.Exactly(ImportStates.Length));
            Proxy.ImportJobController.Mock.Verify(
                x => x.EndAsync(SourceWorkspace.ArtifactId, jobDetails.JobHistoryGuid),
                Times.Once);
            Proxy.ImportJobController.Mock.Verify(
                x => x.CancelAsync(SourceWorkspace.ArtifactId, jobDetails.JobHistoryGuid),
                Times.Never);
        }

        private void VerifyAddToImportQueue(JobTest job, CustomProviderJobDetails jobDetails)
        {
            var batchesCount = _managementTestData.Data.Count / Context.InstanceSettings.CustomProviderBatchSize;

            Proxy.ImportSourceControllerStub.Mock.Verify(
                x => x.AddSourceAsync(
                    SourceWorkspace.ArtifactId,
                    jobDetails.JobHistoryGuid,
                    It.IsAny<Guid>(),
                    It.IsAny<DataSourceSettings>()), Times.Exactly(batchesCount));

            var expectedJobDetailsUpdateExecutionsCount = batchesCount * 2 + 1;
            var jobDetailsUpdateExecutions = QueueQueryManagerMock.JobDetailsUpdateExecutions;

            jobDetailsUpdateExecutions.Count.Should().Be(expectedJobDetailsUpdateExecutionsCount);

            for (int i = 0; i < expectedJobDetailsUpdateExecutionsCount; i++)
            {
                jobDetailsUpdateExecutions[i].Key.ShouldBeEquivalentTo(job.JobId);

                var customProviderJobDetails =
                    Serializer.Deserialize<CustomProviderJobDetails>(jobDetailsUpdateExecutions[i].Value);
                customProviderJobDetails.Batches.Count.ShouldBeEquivalentTo(batchesCount);
                for (int j = 0; j < batchesCount; j++)
                {
                    var isAllBatchesAddedToQueue = j < i;
                    customProviderJobDetails.Batches[j].BatchID.ShouldBeEquivalentTo(j);
                    customProviderJobDetails.Batches[j].IsAddedToImportQueue.ShouldBeEquivalentTo(isAllBatchesAddedToQueue);
                    customProviderJobDetails.Batches[j].NumberOfRecords
                        .ShouldBeEquivalentTo(Context.InstanceSettings.CustomProviderBatchSize);

                    var cutOffIdx = i % (batchesCount + 1);
                    customProviderJobDetails.Batches[j].Status.ShouldBeEquivalentTo( i > batchesCount ? j <= cutOffIdx ? BatchStatus.Completed : BatchStatus.Started : BatchStatus.Started);
                }

                if (i > 0 && i < batchesCount + 1)
                {
                    Proxy.ObjectManager.Mock.Verify(
                        x => x.UpdateAsync(
                            SourceWorkspace.ArtifactId,
                            It.Is<UpdateRequest>(
                                y =>
                                    y.Object.ArtifactID == jobDetails.JobHistoryID &&
                                    y.FieldValues.FirstOrDefault().Field.Guid == JobHistoryFieldGuids.ItemsReadGuid &&
                                    (int)y.FieldValues.FirstOrDefault().Value ==
                                    Context.InstanceSettings.CustomProviderBatchSize * i)),
                        Times.Once);
                }
            }
        }

        private JobTest ScheduleImportCustomProviderJob()
        {
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint();

            Helper.SecretStore.Setup(SourceWorkspace, integrationPoint);

            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncManagerJob(SourceWorkspace, integrationPoint, _managementTestData.EntryIds);

            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            return job;
        }

        private void VerifyJobHistoryStatus(Guid expectedStatusGuid)
        {
            JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();
            jobHistory.JobStatus.Guids.Single().Should().Be(expectedStatusGuid);
        }

        private void VerifyFileExistenceAndContent(JobTest job, CustomProviderJobDetails jobDetails)
        {
            string idFullFilePath = jobDetails.Batches.First().IDsFilePath;

            File.Exists(idFullFilePath).Should().BeTrue();
            IsFileEmpty(idFullFilePath).Should().BeFalse();

            string dataFullFilePath = Path.Combine(Path.GetDirectoryName(idFullFilePath), Path.GetFileNameWithoutExtension(idFullFilePath) + ".data");
            File.Exists(dataFullFilePath).Should().BeTrue();
            IsFileEmpty(dataFullFilePath).Should().BeFalse();
        }

        private bool IsFileEmpty(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length == 0;
        }

        private CustomProviderJobDetails GetJobDetails(string details)
        {
            var serializer = new JSONSerializer();
            CustomProviderJobDetails jobDetails = serializer.Deserialize<CustomProviderJobDetails>(details);
            return jobDetails;
        }
    }
}
