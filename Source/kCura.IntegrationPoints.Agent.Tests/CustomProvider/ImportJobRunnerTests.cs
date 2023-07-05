using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class ImportJobRunnerTests
    {
        private const int WorkspaceId = 111;

        private Mock<IImportApiService> _importApiService;
        private Mock<IJobDetailsService> _jobDetailsService;
        private Mock<IIdFilesBuilder> _idFilesBuilder;
        private Mock<ILoadFileBuilder> _loadFileBuilder;
        private Mock<IRelativityStorageService> _relativityStorageService;
        private Mock<IImportApiRunnerFactory> _importApiRunnerFactory;
        private Mock<IJobProgressHandler> _jobProgressHandler;
        private Mock<IJobHistoryService> _jobHistoryService;
        private Mock<IItemLevelErrorHandler> _itemLevelErrorHandler;

        private Mock<IStorageAccess<string>> _storageAccess;
        private Mock<IImportApiRunner> _importApiRunner;
        private Mock<IDisposable> _jobProgressUpdater;
        private Mock<IDataSourceProvider> _sourceProvider;

        private IntegrationPointDto _integrationPointDto;

        [SetUp]
        public void SetUp()
        {
            _importApiService = new Mock<IImportApiService>();
            _jobDetailsService = new Mock<IJobDetailsService>();
            _idFilesBuilder = new Mock<IIdFilesBuilder>();
            _loadFileBuilder = new Mock<ILoadFileBuilder>();

            _relativityStorageService = new Mock<IRelativityStorageService>();
            _relativityStorageService
                .Setup(x => x.PrepareImportDirectoryAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new DirectoryInfo("import-directory"));

            _storageAccess = new Mock<IStorageAccess<string>>();

            _relativityStorageService
                .Setup(x => x.GetStorageAccessAsync())
                .ReturnsAsync(_storageAccess.Object);

            _importApiRunnerFactory = new Mock<IImportApiRunnerFactory>();

            _importApiRunner = new Mock<IImportApiRunner>();

            _importApiRunnerFactory
                .Setup(x => x.BuildRunner(It.IsAny<DestinationConfiguration>()))
                .Returns(_importApiRunner.Object);

            _jobProgressUpdater = new Mock<IDisposable>();

            _jobProgressHandler = new Mock<IJobProgressHandler>();
            _jobProgressHandler
                .Setup(x => x.BeginUpdateAsync(It.IsAny<ImportJobContext>()))
                .ReturnsAsync(_jobProgressUpdater.Object);

            _itemLevelErrorHandler = new Mock<IItemLevelErrorHandler>();

            _jobHistoryService = new Mock<IJobHistoryService>();
            _sourceProvider = new Mock<IDataSourceProvider>();

            _integrationPointDto = new IntegrationPointDto
            {
                DestinationConfiguration = new DestinationConfiguration { CaseArtifactId = WorkspaceId },
                FieldMappings = Enumerable.Range(0, 3).Select(x => new FieldMap()).ToList()
            };
        }

        [Test]
        public async Task RunJobAsync_GoldFlow()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            Guid batchInstance = Guid.NewGuid();
            const int numberOfBatches = 3;

            List<CustomProviderBatch> batches = Enumerable
                .Range(0, numberOfBatches)
                .Select(x => new CustomProviderBatch()
                {
                    BatchID = x
                })
                .ToList();

            JobHistory jobHistory = new JobHistory()
            {
                ArtifactId = jobHistoryId
            };

            Job job = PrepareJob(workspaceId, batchInstance);

            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                Batches = batches,
                JobHistoryGuid = batchInstance,
                JobHistoryID = jobHistoryId
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

            _importApiService.Setup(x => x.GetJobImportStatusAsync(It.IsAny<ImportJobContext>()))
                .ReturnsAsync(new ImportDetails(ImportState.Completed, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<DateTime>()));

            _importApiService.Setup(x => x.GetDataSourceDetailsAsync(It.IsAny<ImportJobContext>(), It.IsAny<Guid>()))
                .ReturnsAsync(new DataSourceDetails() { State = DataSourceState.Completed });

            ImportJobRunner sut = PrepareSut();

            // Act
            await sut.RunJobAsync(job, jobDetails, _integrationPointDto, _sourceProvider.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            _relativityStorageService
                .Verify(x => x.DeleteDirectoryRecursiveAsync(It.IsAny<string>()), Times.Once);

            _importApiRunner.Verify(x => x.RunImportJobAsync(It.IsAny<ImportJobContext>(), It.IsAny<DestinationConfiguration>(), It.IsAny<List<IndexedFieldMap>>()),
                Times.Once);

            _importApiService
                .Verify(x => x.AddDataSourceAsync(It.IsAny<ImportJobContext>(), It.IsAny<Guid>(), It.IsAny<DataSourceSettings>()),
                Times.Exactly(numberOfBatches));

            _jobProgressUpdater.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void Execute_ShouldCleanupImportDirectory_WhenExceptionIsThrown()
        {
            // Arrange
            Guid batchInstance = Guid.NewGuid();
            int jobHistoryId = 222;

            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                Batches = new List<CustomProviderBatch>(),
                JobHistoryGuid = batchInstance,
                JobHistoryID = jobHistoryId
            };

            JobHistory jobHistory = new JobHistory();

            _idFilesBuilder
                .Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<CustomProviderBatch>());

            _jobHistoryService
                .Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(jobHistory);

            Job job = PrepareJob(WorkspaceId, Guid.NewGuid());

            ImportJobRunner sut = PrepareSut();

            // Act
            Func<Task> action = async () => await sut.RunJobAsync(job, jobDetails, new IntegrationPointDto(), Mock.Of<IDataSourceProvider>(), CompositeCancellationToken.None);

            // Assert
            action.ShouldThrow<Exception>();
            _relativityStorageService
                .Verify(x => x.DeleteDirectoryRecursiveAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Execute_ShouldNotCleanupImportDirectory_WhenDrainStopped()
        {
            // Arrange
            Guid batchInstance = Guid.NewGuid();
            int jobHistoryId = 222;

            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                Batches = new List<CustomProviderBatch>(),
                JobHistoryGuid = batchInstance,
                JobHistoryID = jobHistoryId
            };

            JobHistory jobHistory = new JobHistory();

            _idFilesBuilder
                .Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<CustomProviderBatch>());

            _jobHistoryService
                .Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(jobHistory);

            _importApiService.Setup(x => x.GetJobImportStatusAsync(It.IsAny<ImportJobContext>()))
                .ReturnsAsync(new ImportDetails(ImportState.Completed, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<DateTime>()));

            _importApiService.Setup(x => x.GetDataSourceDetailsAsync(It.IsAny<ImportJobContext>(), It.IsAny<Guid>()))
                .ReturnsAsync(new DataSourceDetails() { State = DataSourceState.Completed });

            CancellationTokenSource cancelToken = new CancellationTokenSource();
            CancellationTokenSource drainStopToken = new CancellationTokenSource();

            CompositeCancellationToken token = new CompositeCancellationToken(cancelToken.Token, drainStopToken.Token, Mock.Of<IAPILog>());

            Job job = PrepareJob(WorkspaceId, Guid.NewGuid());

            ImportJobRunner sut = PrepareSut();

            // Act
            drainStopToken.Cancel();
            await sut.RunJobAsync(job, jobDetails, _integrationPointDto, Mock.Of<IDataSourceProvider>(), token);

            // Assert
            _relativityStorageService
                .Verify(x => x.DeleteDirectoryRecursiveAsync(It.IsAny<string>()), Times.Never);
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

        private ImportJobRunner PrepareSut()
        {
            return new ImportJobRunner(
                _importApiService.Object,
                _jobDetailsService.Object,
                _loadFileBuilder.Object,
                _relativityStorageService.Object,
                _importApiRunnerFactory.Object,
                _jobProgressHandler.Object,
                _itemLevelErrorHandler.Object,
                Mock.Of<IAPILog>());
        }
    }
}
