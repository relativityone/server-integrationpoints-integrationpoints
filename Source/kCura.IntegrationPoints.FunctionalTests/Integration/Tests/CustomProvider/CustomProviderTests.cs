using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Common.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.CustomProvider
{
    [TestExecutionCategory.CI]
    [TestLevel.L2]
    public class CustomProviderTests : TestsBase
    {
        private readonly ManagementTestData _managementTestData = new ManagementTestData();

        public override void SetUp()
        {
            base.SetUp();

            DataTable result = new DataTable
            {
                Columns = { new DataColumn() }
            };

            Helper.DbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);
        }


        [Test]
        public void CustomProviderTest_SystemTest()
        {
            // Arrange
            var job = ScheduleImportCustomProviderJob();

            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) => { importJob.Complete(_managementTestData.Data.Count); })).LifestyleSingleton());

            FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

            // Act
            sut.Execute();

            // Assert
            VerifyJobHistoryStatus(JobStatusChoices.JobHistoryPendingGuid);
            VerifyFileExistanceAndContent(job);

            FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
        }

        private JobTest ScheduleImportCustomProviderJob()
        {
            Context.ToggleValues.SetValue<EnableImportApiV2ForCustomProvidersToggle>(true);

            Context.InstanceSettings.CustomProviderBatchSize = 1000;

            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportObjectFromLdapIntegrationPoint();

            Helper.SecretStore.Setup(SourceWorkspace, integrationPoint);

            JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncManagerJob(SourceWorkspace, integrationPoint, _managementTestData.EntryIds);

            JobHistoryTest jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            InsertBatchToJobTrackerTable(job, jobHistory);
            return job;
        }

        private void InsertBatchToJobTrackerTable(JobTest job, JobHistoryTest jobHistory)
        {
            string tableName = string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, jobHistory.BatchInstance);

            if (!FakeRelativityInstance.JobTrackerResourceTables.ContainsKey(tableName))
            {
                FakeRelativityInstance.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
            }

            FakeRelativityInstance.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = job.JobId });
        }

        private void VerifyJobHistoryStatus(Guid expectedStatusGuid)
        {
            JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();
            jobHistory.JobStatus.Guids.Single().Should().Be(expectedStatusGuid);
        }

        private void VerifyFileExistanceAndContent(JobTest job)
        {
            CustomProviderJobDetails jobDetails = GetJobDetails(job.JobDetails);
            string idFullFilePath = jobDetails.Batches.First().IDsFilePath;

            File.Exists(idFullFilePath).Should().BeTrue();
            FileShouldNotBeEmpty(idFullFilePath).Should().BeTrue();

            string dataFullFilePath = Path.Combine(Path.GetDirectoryName(idFullFilePath), Path.GetFileNameWithoutExtension(idFullFilePath) + ".data");
            File.Exists(dataFullFilePath).Should().BeTrue();
            FileShouldNotBeEmpty(dataFullFilePath).Should().BeTrue();
        }

        private bool FileShouldNotBeEmpty(string filePath)
        {
            Stream stream = File.OpenRead(filePath);
            string fileContent;

            using (TextReader reader = new StreamReader(stream))
            {
                fileContent = reader.ReadToEnd();
            }

            return fileContent != string.Empty;
        }

        private CustomProviderJobDetails GetJobDetails(string details)
        {
            var serializer = new JSONSerializer();
            CustomProviderJobDetails jobDetails = serializer.Deserialize<CustomProviderJobDetails>(details);
            return jobDetails;
        }
    }
}
