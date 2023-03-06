using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
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
    public class CustomProviderTaskTests
    {
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

        [SetUp]
        public void SetUp()
        {
            _serviceFactory = new Mock<IKeplerServiceFactory>();
            _integrationPointService = new Mock<IIntegrationPointService>();
            _sourceProviderService = new Mock<ISourceProviderService>();
            _idFilesBuilder = new Mock<IIdFilesBuilder>();
            _loadFileBuilder = new Mock<ILoadFileBuilder>();

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

            _serviceFactory
                .Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobController.Object);

            IntegrationPointDto dto = new IntegrationPointDto();
            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(dto);
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // Arrange
            const int destinationWorkspaceId = 111;
            const int numberOfBatches = 3;

            ImportSettings destinationConfiguration = new ImportSettings()
            {
                CaseArtifactId = destinationWorkspaceId
            };

            IntegrationPointDto integrationPointDto = new IntegrationPointDto()
            {
                DestinationConfiguration = new JSONSerializer().Serialize(destinationConfiguration),
                FieldMappings = Enumerable.Range(0, 3).Select(x => new FieldMap()).ToList()
            };

            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(integrationPointDto);

            List<CustomProviderBatch> batches = Enumerable.Range(0, numberOfBatches).Select(x => new CustomProviderBatch()
            {
                BatchID = x
            }).ToList();

            _idFilesBuilder
                .Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(), It.IsAny<string>()))
                .ReturnsAsync(batches);

            _loadFileBuilder
                .Setup(x => x.CreateDataFileAsync(It.IsAny<CustomProviderBatch>(), It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(),
                    It.IsAny<string>(), It.IsAny<List<IndexedFieldMap>>()))
                .ReturnsAsync(new DataSourceSettings());

            _importSourceController
                .Setup(x => x.AddSourceAsync(destinationWorkspaceId, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()))
                .ReturnsAsync(new Response(Guid.Empty, true, string.Empty, string.Empty));

            _importJobController
                .Setup(x => x.GetDetailsAsync(destinationWorkspaceId, It.IsAny<Guid>()))
                .ReturnsAsync(new ValueResponse<ImportDetails>(Guid.Empty, true, string.Empty, string.Empty, new ImportDetails(ImportState.Scheduled, string.Empty, 777, DateTime.Now, 777, DateTime.Now)));

            _importJobController
                .Setup(x => x.EndAsync(destinationWorkspaceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Response(Guid.Empty, true, string.Empty, string.Empty));

            Job job = new Job()
            {
                JobDetails = string.Empty
            };

            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _jobService.Verify(x => x.UpdateJobDetails(It.Is<Job>(storedJob => VerifyJob(storedJob, numberOfBatches))),
                Times.Exactly(numberOfBatches + 1)); // one after creating batches + after adding each data source

            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _importApiRunner.Verify(x => x.RunImportJobAsync(It.IsAny<ImportJobContext>(), It.IsAny<ImportSettings>(), It.IsAny<List<IndexedFieldMap>>()),
                Times.Once);

            _importSourceController.Verify(x => x.AddSourceAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()),
                Times.Exactly(numberOfBatches));

        }

        [Test]
        public void Execute_ShouldCleanupImportDirectory_WhenExceptionIsThrown()
        {
            // Arrange
            _integrationPointService
                .Setup(x => x.Read(It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            Job job = new Job();

            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        private bool VerifyJob(Job job, int numberOfBatches)
        {
            CustomProviderJobDetails jobDetails = new JSONSerializer().Deserialize<CustomProviderJobDetails>(job.JobDetails);
            jobDetails.ImportJobID.Should().NotBe(Guid.Empty);
            jobDetails.Batches.Count.Should().Be(numberOfBatches);
            return true;
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
                Mock.Of<IAPILog>());
        }
    }
}
