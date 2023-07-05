using System;
using System.Data;
using System.IO;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Common.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using FileInfo = System.IO.FileInfo;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.CustomProvider
{
    [TestExecutionCategory.CI]
    [TestLevel.L2]
    public class CustomProviderTests : TestsBase
    {
        private readonly ManagementTestData _managementTestData = new ManagementTestData();

        /* 
        Basic implementation:
            IAPI 2.0 calls
            job history updates
            status updates
            
        Additional tests:
            k8s crashes
            IdFilesBuilder - partly done
            

        */

        public override void SetUp()
        {
            base.SetUp();

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);

            Context.ToggleValues.SetValue<EnableImportApiV2ForCustomProvidersToggle>(true);

            Context.InstanceSettings.CustomProviderBatchSize = 5;
            Context.InstanceSettings.CustomProviderProgressUpdateInterval = TimeSpan.FromSeconds(100);
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
            VerifyJobHistoryStatus(JobStatusChoices.JobHistoryPendingGuid);
            VerifyFileExistenceAndContent(job);

            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
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

        private void VerifyFileExistenceAndContent(JobTest job)
        {
            CustomProviderJobDetails jobDetails = GetJobDetails(job.JobDetails);
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
