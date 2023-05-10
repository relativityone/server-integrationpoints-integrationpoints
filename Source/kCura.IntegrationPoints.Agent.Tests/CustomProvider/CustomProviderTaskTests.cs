using System;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class CustomProviderTaskTests
    {
        private const int WorkspaceId = 111;
        private const int SourceProviderId = 222;

        private Mock<ICancellationTokenFactory> _cancellationTokenFactory;
        private Mock<IAgentValidator> _agentValidator;
        private Mock<IJobDetailsService> _jobDetailsService;
        private Mock<IIntegrationPointService> _integrationPointService;
        private Mock<ISourceProviderService> _sourceProviderService;
        private Mock<IImportJobRunner> _importJobRunner;
        private Mock<IAPILog> _logger;

        [SetUp]
        public void SetUp()
        {
            _cancellationTokenFactory = new Mock<ICancellationTokenFactory>();
            _agentValidator = new Mock<IAgentValidator>();
            _jobDetailsService = new Mock<IJobDetailsService>();
            _integrationPointService = new Mock<IIntegrationPointService>();
            _sourceProviderService = new Mock<ISourceProviderService>();
            _importJobRunner = new Mock<IImportJobRunner>();
            _logger = new Mock<IAPILog>();

            var destinationConfiguration = new DestinationConfiguration()
            {
                CaseArtifactId = WorkspaceId
            };

            IntegrationPointDto integrationPointDto = new IntegrationPointDto()
            {
                DestinationConfiguration = destinationConfiguration,
                FieldMappings = Enumerable.Range(0, 3).Select(x => new FieldMap()).ToList(),
                SourceProvider = SourceProviderId
            };

            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(integrationPointDto);
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // Arrange
            const int jobId = 5;
            Guid batchInstance = Guid.NewGuid();

            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                JobHistoryGuid = batchInstance
            };

            _jobDetailsService.Setup(x => x.GetJobDetailsAsync(WorkspaceId, It.IsAny<string>()))
                .ReturnsAsync(jobDetails);
            Job job = PrepareJob(WorkspaceId, batchInstance, jobId);

            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _agentValidator.Verify(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()), Times.Once);
            _jobDetailsService.Verify(x => x.GetJobDetailsAsync(WorkspaceId, It.Is<string>(str => str == job.JobDetails)));
            _sourceProviderService.Verify(x => x.GetSourceProviderAsync(WorkspaceId, SourceProviderId));
            _cancellationTokenFactory.Verify(x => x.GetCancellationToken(batchInstance, job.JobId));

            _importJobRunner.Verify(x => x.RunJobAsync(
                It.IsAny<Job>(),
                It.IsAny<CustomProviderJobDetails>(),
                It.IsAny<IntegrationPointDto>(),
                It.IsAny<IDataSourceProvider>(),
                It.IsAny<CompositeCancellationToken>()));
        }

        [Test]
        public void Execute_ShouldNotExecuteJob_WhenValidationFails()
        {
            // Arrange
            IntegrationPointValidationException exception = new IntegrationPointValidationException(new ValidationResult());
            _agentValidator
                .Setup(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()))
                .Throws(exception);

            Job job = new Job();
            CustomProviderTask sut = PrepareSut();

            // Act & Assert
            Assert.Throws<IntegrationPointValidationException>(() => sut.Execute(job));
            _agentValidator.Verify(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()), Times.Once);

            _jobDetailsService.Verify(x => x.GetJobDetailsAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _sourceProviderService.Verify(x => x.GetSourceProviderAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _cancellationTokenFactory.Verify(x => x.GetCancellationToken(It.IsAny<Guid>(), It.IsAny<long>()), Times.Never);

            _importJobRunner.Verify(
                x => x.RunJobAsync(
                    It.Is<Job>(j => j == job),
                    It.IsAny<CustomProviderJobDetails>(),
                    It.IsAny<IntegrationPointDto>(),
                    It.IsAny<IDataSourceProvider>(),
                    It.IsAny<CompositeCancellationToken>()),
                Times.Never);
        }

        private Job PrepareJob(int workspaceId, Guid jobHistoryGuid, int jobId)
        {
            Job job = new Job()
            {
                JobDetails = new JSONSerializer().Serialize(new TaskParameters()
                {
                    BatchInstance = jobHistoryGuid
                }),
                WorkspaceID = workspaceId,
                JobId = jobId,
            };

            return job;
        }

        private CustomProviderTask PrepareSut()
        {
            return new CustomProviderTask(
                _cancellationTokenFactory.Object,
                _agentValidator.Object,
                _jobDetailsService.Object,
                _integrationPointService.Object,
                _sourceProviderService.Object,
                _importJobRunner.Object,
                _logger.Object);
        }

        private bool VerifyJob(Job job, int numberOfBatches)
        {
            CustomProviderJobDetails jobDetails = new JSONSerializer().Deserialize<CustomProviderJobDetails>(job.JobDetails);
            jobDetails.JobHistoryGuid.Should().NotBe(Guid.Empty);
            jobDetails.Batches.Count.Should().Be(numberOfBatches);
            return true;
        }
    }
}
