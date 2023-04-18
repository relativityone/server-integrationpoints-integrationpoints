using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class CustomProviderTaskTests
    {
        private const int _destinationWorkspaceId = 111;

        private Mock<IKeplerServiceFactory> _serviceFactory;
        private Mock<IIntegrationPointService> _integrationPointService;
        private Mock<ISourceProviderService> _sourceProviderService;
        private Mock<IIdFilesBuilder> _idFilesBuilder;
        private Mock<ILoadFileBuilder> _loadFileBuilder;
        private Mock<IRelativityStorageService> _relativityStorageService;
        private Mock<IStorageAccess<string>> _storageAccess;
        private Mock<IJobService> _jobService;
        private Mock<IImportApiRunnerFactory> _importApiRunnerFactory;
        private Mock<IImportSourceController> _importSourceController;
        private Mock<IImportApiRunner> _importApiRunner;
        private Mock<IImportJobController> _importJobController;
        private Mock<IJobProgressHandler> _jobProgressHandler;
        private Mock<IJobHistoryService> _jobHistoryService;
        private Mock<IDisposable> _jobProgressUpdater;
        private Mock<IAgentValidator> _agentValidator;

        [SetUp]
        public void SetUp()
        {
            _serviceFactory = new Mock<IKeplerServiceFactory>();
            _integrationPointService = new Mock<IIntegrationPointService>();
            _sourceProviderService = new Mock<ISourceProviderService>();
            _idFilesBuilder = new Mock<IIdFilesBuilder>();
            _loadFileBuilder = new Mock<ILoadFileBuilder>();
            _agentValidator = new Mock<IAgentValidator>();

            _relativityStorageService = new Mock<IRelativityStorageService>();
            _relativityStorageService
                .Setup(x => x.GetWorkspaceDirectoryPathAsync(It.IsAny<int>()))
                .ReturnsAsync("test-directory");

            _storageAccess = new Mock<IStorageAccess<string>>();
            _relativityStorageService
                .Setup(x => x.GetStorageAccessAsync())
                .ReturnsAsync(_storageAccess.Object);

            _jobService = new Mock<IJobService>();
            _importApiRunnerFactory = new Mock<IImportApiRunnerFactory>();

            _importSourceController = new Mock<IImportSourceController>();

            _serviceFactory
                .Setup(x => x.CreateProxyAsync<IImportSourceController>())
                .ReturnsAsync(_importSourceController.Object);

            _importApiRunner = new Mock<IImportApiRunner>();

            _importApiRunnerFactory
                .Setup(x => x.BuildRunner(It.IsAny<ImportSettings>()))
                .Returns(_importApiRunner.Object);

            _importJobController = new Mock<IImportJobController>();
            _importJobController
                .Setup(x => x.CancelAsync(_destinationWorkspaceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Response(Guid.Empty, true, string.Empty, string.Empty));

            _serviceFactory
                .Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobController.Object);

            _jobProgressUpdater = new Mock<IDisposable>();
            _jobProgressHandler = new Mock<IJobProgressHandler>();
            _jobProgressHandler
                .Setup(x => x.BeginUpdateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(_jobProgressUpdater.Object);

            _jobHistoryService = new Mock<IJobHistoryService>();

            ImportSettings destinationConfiguration = new ImportSettings()
            {
                CaseArtifactId = _destinationWorkspaceId
            };

            IntegrationPointDto integrationPointDto = new IntegrationPointDto()
            {
                DestinationConfiguration = new JSONSerializer().Serialize(destinationConfiguration),
                FieldMappings = Enumerable.Range(0, 3).Select(x => new FieldMap()).ToList()
            };

            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(integrationPointDto);
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            Guid batchInstance = Guid.NewGuid();
            const int numberOfBatches = 3;

            List<CustomProviderBatch> batches = Enumerable.Range(0, numberOfBatches).Select(x => new CustomProviderBatch()
            {
                BatchID = x
            }).ToList();

            JobHistory jobHistory = new JobHistory()
            {
                ArtifactId = jobHistoryId
            };

            _jobHistoryService
                .Setup(x => x.ReadJobHistoryByGuidAsync(workspaceId, batchInstance))
                .ReturnsAsync(jobHistory);

            _idFilesBuilder
                .Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(), It.IsAny<string>()))
                .ReturnsAsync(batches);

            _loadFileBuilder
                .Setup(x => x.CreateDataFileAsync(It.IsAny<CustomProviderBatch>(), It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointInfo>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new DataSourceSettings());

            _importSourceController
                .Setup(x => x.AddSourceAsync(_destinationWorkspaceId, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()))
                .ReturnsAsync(new Response(Guid.Empty, true, string.Empty, string.Empty));

            _importJobController
                .Setup(x => x.GetDetailsAsync(_destinationWorkspaceId, It.IsAny<Guid>()))
                .ReturnsAsync(new ValueResponse<ImportDetails>(Guid.Empty, true, string.Empty, string.Empty, new ImportDetails(ImportState.Scheduled, string.Empty, 777, DateTime.Now, 777, DateTime.Now)));

            _importJobController
                .Setup(x => x.EndAsync(_destinationWorkspaceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Response(Guid.Empty, true, string.Empty, string.Empty));

            Job job = PrepareJob(workspaceId, batchInstance);
            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _agentValidator.Verify(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()), Times.Once);

            _jobService.Verify(x => x.UpdateJobDetails(It.Is<Job>(storedJob => VerifyJob(storedJob, numberOfBatches))),
                Times.Exactly(numberOfBatches + 1)); // one after creating batches + after adding each data source

            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _importApiRunner.Verify(x => x.RunImportJobAsync(It.IsAny<ImportJobContext>(), It.IsAny<ImportSettings>(), It.IsAny<List<IndexedFieldMap>>()),
                Times.Once);

            _importSourceController.Verify(x => x.AddSourceAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()),
                Times.Exactly(numberOfBatches));

            _jobProgressUpdater.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void Execute_ShouldCleanupImportDirectory_WhenExceptionIsThrown()
        {
            // Arrange

            JobHistory jobHistory = new JobHistory();

            _jobHistoryService
                .Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(jobHistory);

            _sourceProviderService
                .Setup(x => x.GetSourceProviderAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            Job job = PrepareJob(111, Guid.NewGuid());

            CustomProviderTask sut = PrepareSut();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(job));

            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public void Execute_ShouldNotExecuteJob_WhenValidationFails()
        {
            // Arrange
            IntegrationPointValidationException exception = new IntegrationPointValidationException(new ValidationResult());
            _agentValidator.Setup(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>())).Throws(exception);

            Job job = new Job();
            CustomProviderTask sut = PrepareSut();

            // Act & Assert
            Assert.Throws<IntegrationPointValidationException>(() => sut.Execute(job));
            _agentValidator.Verify(x => x.Validate(It.IsAny<IntegrationPointDto>(), It.IsAny<int>()), Times.Once);
            _jobService.Verify(x => x.UpdateJobDetails(It.IsAny<Job>()), Times.Never);

            _storageAccess.Verify(
                x => x.DeleteDirectoryAsync(
                    It.IsAny<string>(),
                    It.IsAny<DeleteDirectoryOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _importApiRunner.Verify(
                x => x.RunImportJobAsync(
                    It.IsAny<ImportJobContext>(),
                    It.IsAny<ImportSettings>(),
                    It.IsAny<List<IndexedFieldMap>>()),
                Times.Never);

            _importSourceController.Verify(
                x => x.AddSourceAsync(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DataSourceSettings>()),
                Times.Never);
        }

        private bool VerifyJob(Job job, int numberOfBatches)
        {
            CustomProviderJobDetails jobDetails = new JSONSerializer().Deserialize<CustomProviderJobDetails>(job.JobDetails);
            jobDetails.ImportJobID.Should().NotBe(Guid.Empty);
            jobDetails.Batches.Count.Should().Be(numberOfBatches);
            return true;
        }

        private Job PrepareJob(int workspaceId, Guid batchInstance)
        {
            Job job = new Job()
            {
                JobDetails = new JSONSerializer().Serialize(new TaskParameters()
                {
                    BatchInstance = batchInstance
                }),
                WorkspaceID = workspaceId
            };

            return job;
        }

        private CustomProviderTask PrepareSut()
        {
            return new CustomProviderTask(
                _serviceFactory.Object,
                _integrationPointService.Object,
                _sourceProviderService.Object,
                _idFilesBuilder.Object,
                _loadFileBuilder.Object,
                _relativityStorageService.Object,
                new JSONSerializer(),
                _jobService.Object,
                _importApiRunnerFactory.Object,
                _jobProgressHandler.Object,
                _jobHistoryService.Object,
                _agentValidator.Object,
                Mock.Of<IAPILog>());
        }
    }
}
