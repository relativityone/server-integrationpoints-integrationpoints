using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Agent.Tests.TaskFactory
{
    [TestFixture, Category("Unit")]
    public class TaskFactoryJobHistoryServiceTests
    {
        private ITaskFactoryJobHistoryService _sut;
        private IJobHistoryService _jobHistoryService;
        private IJobHistoryErrorService _jobHistoryErrorService;
        private IIntegrationPointService _integrationPointService;

        public void SetUp(IntegrationPointDto ip = null)
        {
            IAPILog logger = Substitute.For<IAPILog>();
            ISerializer serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<ImportSettings>(Arg.Any<string>()).Returns(new ImportSettings());
            serializer.Deserialize<TaskParameters>(Arg.Any<string>()).Returns(new TaskParameters());

            _jobHistoryService = Substitute.For<IJobHistoryService>();
            _jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();

            ip = ip ?? GetDefaultIntegrationPoint();
            _sut = new TaskFactoryJobHistoryService(
                logger,
                serializer,
                _jobHistoryErrorService,
                _integrationPointService,
                _jobHistoryService,
                ip
            );
        }

        [Test]
        public void ItShouldUpdateJobHistoryWhenItDoesnNotHaveValue()
        {
            SetUp();
            // Arrange
            int jobId = 123;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            var jobHistory = new JobHistory
            {
                JobID = null
            };
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.Received().UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(h => h.JobID == jobId.ToString()));
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenItHasValue()
        {
            SetUp();
            // Arrange
            int jobId = 123;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            var jobHistory = new JobHistory
            {
                JobID = jobId.ToString()
            };
            _jobHistoryService.CreateRdo(Arg.Any<IntegrationPointDto>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdoWithoutDocuments(Arg.Any<JobHistory>());
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenHobHistoryServiceReturnNull()
        {
            SetUp();
            // Arrange
            Job job = JobExtensions.CreateJob();
            _jobHistoryService.CreateRdo(Arg.Any<IntegrationPointDto>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns((JobHistory)null);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdoWithoutDocuments(Arg.Any<JobHistory>());
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenJobWasNull()
        {
            // Arrange
            SetUp();
            Job job = null;
            var jobHistory = new JobHistory();
            _jobHistoryService.CreateRdo(Arg.Any<IntegrationPointDto>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdoWithoutDocuments(Arg.Any<JobHistory>());
        }

        [TestCase(new[] { 123 }, 123, new int[] { })]
        [TestCase(new[] { 1, 2 }, 2, new[] { 1 })]
        [TestCase(new[] { 1, 2 }, 3, new[] { 1, 2 })]
        [TestCase(new int[] { }, 3, new int[] { })]
        public void ItShouldUpdateIntegrationPointWhenRemovingJobHistoryFromIntegrationPoint(int[] ipJobHistoryIds, int jobHistoryId, int[] expectedIpJobHistoryIdsAfterUpdate)
        {
            // Arrange
            IntegrationPointDto ip = GetDefaultIntegrationPoint();
            ip.JobHistory = ipJobHistoryIds.ToList();
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryId
            };
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _integrationPointService.Received()
                .UpdateJobHistory(Arg.Is<int>(x => true), Arg.Is<List<int>>(x => x.SequenceEqual(expectedIpJobHistoryIdsAfterUpdate)));
        }

        [Test]
        public void ItShouldUpdateJobHistoryStatusWhenRemovingJobHistoryFromIntegrationPoint()
        {
            // Arrange
            IntegrationPointDto ip = GetDefaultIntegrationPoint();
            ip.JobHistory = new List<int>();
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            var jobHistory = new JobHistory();
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _jobHistoryService.Received().UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped)));
        }

        [Test]
        public void ItShouldDeleteJobHistoryWhenRemovingJobHistoryFromIntegrationPoint()
        {
            // Arrange
            IntegrationPointDto ip = GetDefaultIntegrationPoint();
            ip.JobHistory = new List<int>();
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            int jobHistoryArtifactId = 654;
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryArtifactId
            };
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _jobHistoryService.Received().DeleteRdo(Arg.Is<int>(x => x == jobHistoryArtifactId));
        }

        [Test]
        public void ItShouldUpdateJobHistoryStatusOnFailure()
        {
            // Arrange
            SetUp();

            Job job = JobExtensions.CreateJob();
            int jobHistoryArtifactId = 654;
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryArtifactId
            };
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            // Act
            _sut.UpdateJobHistoryOnFailure(job, null);

            // Assert
            _jobHistoryService.Received().UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x.JobStatus.Name == JobStatusChoices.JobHistoryErrorJobFailed.Name));
        }

        [Test]
        public void ItShouldAddJobHistoryErrorOnFailure()
        {
            // Arrange
            int integrationPointArtifactId = 943438;
            var ip = GetDefaultIntegrationPoint();
            ip.ArtifactId = integrationPointArtifactId;
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            var jobHistory = new JobHistory
            {
                ArtifactId = 654,
            };
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(jobHistory);

            string expectedExceptionMessage = "Expected exception message";
            var exception = new Exception(expectedExceptionMessage);

            // Act
            _sut.UpdateJobHistoryOnFailure(job, exception);

            // Assert
            Assert.AreEqual(integrationPointArtifactId, _jobHistoryErrorService.IntegrationPointDto.ArtifactId);
            Assert.AreEqual(jobHistory.ArtifactId, _jobHistoryErrorService.JobHistory.ArtifactId);
            _jobHistoryErrorService.Received().AddError(Arg.Is<ChoiceRef>(x => x.Name == ErrorTypeChoices.JobHistoryErrorJob.Name), exception);
        }

        private IntegrationPointDto GetDefaultIntegrationPoint()
        {
            return new IntegrationPointDto
            {
                DestinationConfiguration = new ImportSettings(),
                SecuredConfiguration = string.Empty
            };
        }
    }
}
