using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class CustomProviderTaskTests
    {
        private Mock<ICancellationTokenFactory> _cancellationTokenFactory;
        private Mock<IAgentValidator> _agentValidator;
        private Mock<IJobDetailsService> _jobDetailsService;
        private Mock<IIntegrationPointService> _integrationPointService;
        private Mock<ISourceProviderService> _sourceProviderService;
        private Mock<IImportJobRunner> _importJobRunner;
        private Mock<IJobHistoryService> _jobHistoryService;
        private Mock<IJobHistoryErrorService> _jobHistoryErrorService;
        private Mock<IIdFilesBuilder> _idFilesBuilder;
        private Mock<IRelativityStorageService> _relativityStorageService;
        private Mock<INotificationService> _notificationService;
        private Mock<IAPILog> _logger;

        private IFixture _fxt;

        private CustomProviderTask _sut;

        private CustomProviderJobDetails _jobDetails;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _cancellationTokenFactory = new Mock<ICancellationTokenFactory>();
            _agentValidator = new Mock<IAgentValidator>();
            _jobDetailsService = new Mock<IJobDetailsService>();
            _integrationPointService = new Mock<IIntegrationPointService>();
            _sourceProviderService = new Mock<ISourceProviderService>();
            _importJobRunner = new Mock<IImportJobRunner>();
            _jobHistoryService = new Mock<IJobHistoryService>();
            _jobHistoryErrorService = new Mock<IJobHistoryErrorService>();
            _idFilesBuilder = new Mock<IIdFilesBuilder>();
            _notificationService = new Mock<INotificationService>();

            _relativityStorageService = new Mock<IRelativityStorageService>();
            _relativityStorageService.Setup(x => x.PrepareImportDirectoryAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new DirectoryInfo(Path.GetTempPath()));

            _logger = new Mock<IAPILog>();

            _jobDetails = _fxt.Create<CustomProviderJobDetails>();

            SetupJobDetails();

            _sut = new CustomProviderTask(
                _cancellationTokenFactory.Object,
                _agentValidator.Object,
                _jobDetailsService.Object,
                _integrationPointService.Object,
                _sourceProviderService.Object,
                _importJobRunner.Object,
                _jobHistoryService.Object,
                _jobHistoryErrorService.Object,
                _idFilesBuilder.Object,
                _relativityStorageService.Object,
                _notificationService.Object,
                _logger.Object);
        }

        [Test]
        public void Execute_ShouldBeValidationFailed_WhenValidationThrows()
        {
            // Arrange
            Job job = PrepareBasicJob();

            _agentValidator.Setup(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()))
                .Throws(new IntegrationPointValidationException(new ValidationResult(false)));

            // Act
            _sut.Execute(job);

            // Assert
            _jobHistoryService.Verify(x => x.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), JobStatusChoices.JobHistoryValidationFailedGuid));

            _jobHistoryErrorService.Verify(
                x => x.AddJobErrorAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<IntegrationPointValidationException>()));
        }

        [Test]
        public void Execute_ShouldConfigureBatches_WhenDoNotExist()
        {
            // Arrange
            _jobDetails = _fxt.Build<CustomProviderJobDetails>()
                .With(x => x.Batches, new List<CustomProviderBatch>())
                .Create();

            Job job = PrepareBasicJob();

            _idFilesBuilder.Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(), It.IsAny<string>()))
                .ReturnsAsync(_fxt.CreateMany<CustomProviderBatch>().ToList());

            SetupImportJobRunner(new ImportJobResult { Status = JobEndStatus.Completed });

            // Act
            _sut.Execute(job);

            // Assert
            _jobDetails.Batches.Should().NotBeEmpty();

            int expectedTotalCount = _jobDetails.Batches.Sum(y => y.NumberOfRecords);

            _jobHistoryService.Verify(x => x.SetTotalItemsAsync(
                It.IsAny<int>(), It.IsAny<int>(), expectedTotalCount));

            _jobHistoryService.Verify(x => x.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), JobStatusChoices.JobHistoryProcessingGuid));
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // Arrange
            List<CustomProviderBatch> batches = _fxt.Build<CustomProviderBatch>()
                .With(x => x.Status, IntegrationPoints.Agent.CustomProvider.DTO.BatchStatus.Completed)
                .CreateMany()
                .ToList();

            _jobDetails = _fxt.Build<CustomProviderJobDetails>()
                .With(x => x.Batches, new List<CustomProviderBatch>())
                .Create();

            Job job = PrepareBasicJob();

            _idFilesBuilder.Setup(x => x.BuildIdFilesAsync(
                    It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(), It.IsAny<string>()))
                .ReturnsAsync(batches);

            SetupImportJobRunner(new ImportJobResult { Status = JobEndStatus.Completed });

            // Act
            _sut.Execute(job);

            // Assert
            _jobHistoryService.Verify(x => x.TryUpdateStartTimeAsync(job.WorkspaceID, _jobDetails.JobHistoryID));
            _jobHistoryService.Verify(x => x.TryUpdateEndTimeAsync(job.WorkspaceID, job.RelatedObjectArtifactID, _jobDetails.JobHistoryID));
            _jobHistoryService.Verify(x => x.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), JobStatusChoices.JobHistoryCompletedGuid));
        }

        private Job PrepareJob(IntegrationPointDto integrationPoint, Guid jobHistoryGuid)
        {
            TaskParameters parameters = _fxt.Build<TaskParameters>()
                .With(x => x.BatchInstance, jobHistoryGuid)
                .Create();

            Job job = _fxt.Build<Job>()
                .With(x => x.JobDetails, new JSONSerializer().Serialize(parameters))
                .With(x => x.RelatedObjectArtifactID, integrationPoint.ArtifactId)
                .With(x => x.WorkspaceID, integrationPoint.DestinationConfiguration.CaseArtifactId)
                .Create();

            return job;
        }

        private Job PrepareBasicJob()
        {
            Guid jobHistoryGuid = Guid.NewGuid();

            IntegrationPointDto integrationPoint = SetupIntegrationPoint();

            return PrepareJob(integrationPoint, jobHistoryGuid);
        }

        private IntegrationPointDto SetupIntegrationPoint()
        {
            IntegrationPointDto integrationPointDto = _fxt.Create<IntegrationPointDto>();

            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(integrationPointDto);

            return integrationPointDto;
        }

        private void SetupImportJobRunner(ImportJobResult result)
        {
            _importJobRunner.Setup(x => x.RunJobAsync(
                    It.IsAny<Job>(),
                    It.IsAny<CustomProviderJobDetails>(),
                    It.IsAny<IntegrationPointDto>(),
                    It.IsAny<ImportJobContext>(),
                    It.IsAny<IDataSourceProvider>(),
                    It.IsAny<CompositeCancellationToken>()))
                .ReturnsAsync(result);
        }

        private void SetupJobDetails()
        {
            _jobDetailsService.Setup(x => x.GetJobDetailsAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(_jobDetails));

            _jobDetailsService.Setup(x => x.UpdateJobDetailsAsync(It.IsAny<Job>(), It.IsAny<CustomProviderJobDetails>()))
                .Returns((Job job, CustomProviderJobDetails jobDetails) =>
                {
                    _jobDetails = jobDetails;

                    return Task.CompletedTask;
                });
        }

        //private bool VerifyJob(Job job, int numberOfBatches)
        //{
        //    CustomProviderJobDetails jobDetails = new JSONSerializer().Deserialize<CustomProviderJobDetails>(job.JobDetails);
        //    jobDetails.JobHistoryGuid.Should().NotBe(Guid.Empty);
        //    jobDetails.Batches.Count.Should().Be(numberOfBatches);
        //    return true;
        //}
    }
}
