using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class FileStatisticsCalculatorTests
    {
        private const int _WORKSPACE_ID = 111;
        private const int _SYNC_STATISTICS_ID = 222;
        private const int _BATCH_SIZE_FOR_FILES_QUERIES = 2;
        private readonly Guid _EXPORT_RUN_ID = Guid.NewGuid();

        private IFileStatisticsCalculator _sut;

        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IImageFileRepository> _imageFileRepositoryFake = new Mock<IImageFileRepository>();
        private Mock<INativeFileRepository> _nativeFileRepositoryFake = new Mock<INativeFileRepository>();
        private Mock<IRdoManager> _rdoManagerMock = new Mock<IRdoManager>();

        [SetUp]
        public void SetUp()
        {
            Mock<IStatisticsConfiguration> configuration = new Mock<IStatisticsConfiguration>();
            configuration.Setup(x => x.SyncStatisticsId).Returns(_SYNC_STATISTICS_ID);
            configuration.Setup(x => x.BatchSizeForFileQueries).Returns(_BATCH_SIZE_FOR_FILES_QUERIES);

            _objectManagerMock = new Mock<IObjectManager>();

            Mock<ISourceServiceFactoryForUser> serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerMock.Object);

            _imageFileRepositoryFake = new Mock<IImageFileRepository>();
            _nativeFileRepositoryFake = new Mock<INativeFileRepository>();

            _rdoManagerMock = new Mock<IRdoManager>();
            _rdoManagerMock.Setup(x => x.GetAsync<SyncStatisticsRdo>(_WORKSPACE_ID, _SYNC_STATISTICS_ID))
                .ReturnsAsync(new SyncStatisticsRdo());

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new FileStatisticsCalculator(
                configuration.Object,
                serviceFactoryForUser.Object,
                _imageFileRepositoryFake.Object,
                _nativeFileRepositoryFake.Object,
                _rdoManagerMock.Object,
                log.Object);
        }

        [Test]
        public async Task CalculateNativesTotalSizeAsync_ShouldCalculateNativesSize_WhenCalculationIsCalledFromScratch()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            // Act
            long result = await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(files.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(files);
        }

        [Test]
        public async Task CalculateImagesTotalSizeAsync_ShouldCalculateNativesSize_WhenCalculationIsCalledFromScratch()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            // Act
            ImagesStatistics result = await _sut
                .CalculateImagesStatisticsAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(),
                    It.IsAny<QueryImagesOptions>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.TotalSize.Should().Be(files.Sum(x => x.Size));
            result.TotalCount.Should().Be(files.Count);

            VerifyCalculatedResultsWasSaved(files);
        }

        [Test]
        public async Task CalculateNativesTotalSizeAsync_ShouldSaveCalculation_WhenDrainStopWasTriggered()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            CompositeCancellationToken token = CreateDrainStopAlwaysCancellationToken();

            // Act
            long result = await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), token)
                .ConfigureAwait(false);

            // Assert
            List<File> expectedCalculatedFiles = files.GetRange(0, _BATCH_SIZE_FOR_FILES_QUERIES).ToList();
            result.Should().Be(expectedCalculatedFiles.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(expectedCalculatedFiles);
        }

        [Test]
        public async Task CalculateImagesTotalSizeAsync_ShouldSaveCalculation_WhenDrainStopWasTriggered()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            CompositeCancellationToken token = CreateDrainStopAlwaysCancellationToken();

            // Act
            ImagesStatistics result = await _sut
                .CalculateImagesStatisticsAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>(), token)
                .ConfigureAwait(false);

            // Assert
            List<File> expectedCalculatedFiles = files.GetRange(0, _BATCH_SIZE_FOR_FILES_QUERIES).ToList();
            result.TotalSize.Should().Be(expectedCalculatedFiles.Sum(x => x.Size));
            result.TotalCount.Should().Be(expectedCalculatedFiles.Count);

            VerifyCalculatedResultsWasSaved(expectedCalculatedFiles);
        }

        [Test]
        public async Task CalculateNativesTotalSizeAsync_ShouldCalculateNativesSize_WhenResuming()
        {
            // Arrange
            const int calculatedFilesCount = 3;

            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            List<File> alreadyCalculatedFiles = files.GetRange(0, calculatedFilesCount).ToList();
            SetupSyncStatistics(new SyncStatisticsRdo
            {
                CalculatedDocuments = alreadyCalculatedFiles.Count,
                RequestedDocuments = files.Count,
                CalculatedFilesSize = alreadyCalculatedFiles.Sum(x => x.Size),
                CalculatedFilesCount = alreadyCalculatedFiles.Count,
                RunId = _EXPORT_RUN_ID
            });

            // Act
            long result = await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(files.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(files);

            int expectedBatchesCount = (int)Math.Ceiling((double)(files.Count - calculatedFilesCount) / _BATCH_SIZE_FOR_FILES_QUERIES);
            VerifyBatchesWasRetrieved(expectedBatchesCount + 1); //Last run returns null
        }

        [Test]
        public async Task CalculateImagesTotalSizeAsync_ShouldCalculateNativesSize_WhenResuming()
        {
            // Arrange
            const int calculatedFilesCount = 3;

            List<File> files = PrepareFiles(10);

            SetupDocuments(files);

            List<File> alreadyCalculatedFiles = files.GetRange(0, calculatedFilesCount).ToList();
            SetupSyncStatistics(new SyncStatisticsRdo
            {
                CalculatedDocuments = alreadyCalculatedFiles.Count,
                RequestedDocuments = files.Count,
                CalculatedFilesSize = alreadyCalculatedFiles.Sum(x => x.Size),
                CalculatedFilesCount = alreadyCalculatedFiles.Count,
                RunId = _EXPORT_RUN_ID
            });

            // Act
            ImagesStatistics result = await _sut
                .CalculateImagesStatisticsAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.TotalCount.Should().Be(files.Count);
            result.TotalSize.Should().Be(files.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(files);

            int expectedBatchesCount = (int)Math.Ceiling((double)(files.Count - calculatedFilesCount) / _BATCH_SIZE_FOR_FILES_QUERIES);
            VerifyBatchesWasRetrieved(expectedBatchesCount + 1); //Last run returns null
        }

        [Test]
        public async Task CalculateNativesTotalSizeAsync_ShouldReturnSavedCalculation_WhenResumingAndPreviouslyCalculationCompleted()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupSyncStatistics(new SyncStatisticsRdo
            {
                CalculatedDocuments = files.Count,
                RequestedDocuments = files.Count,
                CalculatedFilesSize = files.Sum(x => x.Size),
                CalculatedFilesCount = files.Count,
                RunId = _EXPORT_RUN_ID
            });

            // Act
            long result = await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(files.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(files);

            VerifyBatchesWasRetrieved(0);
        }

        [Test]
        public async Task CalculateImagesTotalSizeAsync_ShouldReturnSavedCalculation_WhenResumingAndPreviouslyCalculationCompleted()
        {
            // Arrange
            List<File> files = PrepareFiles(10);

            SetupSyncStatistics(new SyncStatisticsRdo
            {
                CalculatedDocuments = files.Count,
                RequestedDocuments = files.Count,
                CalculatedFilesSize = files.Sum(x => x.Size),
                CalculatedFilesCount = files.Count,
                RunId = _EXPORT_RUN_ID
            });

            // Act
            ImagesStatistics result = await _sut
                .CalculateImagesStatisticsAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.TotalCount.Should().Be(files.Count);
            result.TotalSize.Should().Be(files.Sum(x => x.Size));

            VerifyCalculatedResultsWasSaved(files);

            VerifyBatchesWasRetrieved(0);
        }

        [Test]
        public void CalculateNativesTotalSizeAsync_ShouldNotThrow_WhenExceptionThrown()
        {
            // Act
            Func<Task<long>> action = async () => await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            action.Should().NotThrow();
        }

        [Test]
        public async Task CalculateNativesTotalSizeAsync_ShouldReturnEmptyResults_WhenExceptionThrown()
        {
            // Act
            long result = await _sut
                .CalculateNativesTotalSizeAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Should().Be(0);
        }

        private void SetupDocuments(List<File> files)
        {
            _imageFileRepositoryFake.Setup(x => x.QueryImagesForDocumentsAsync(
                    _WORKSPACE_ID, It.IsAny<int[]>(), It.IsAny<QueryImagesOptions>()))
                .ReturnsAsync((int workspaceId, int[] artifactIds, QueryImagesOptions options) =>
                    files.Where(f => artifactIds.Contains(f.ArtifactId)).Select(f => new ImageFile(0, "", "", "", f.Size)));

            _nativeFileRepositoryFake.Setup(x => x.QueryAsync(
                    _WORKSPACE_ID, It.IsAny<ICollection<int>>()))
                .ReturnsAsync((int workspaceId, ICollection<int> artifactIds) =>
                    files.Where(f => artifactIds.Contains(f.ArtifactId)).Select(f => new NativeFile(0, "", "", f.Size)));

            _objectManagerMock.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1))
                .ReturnsAsync(new ExportInitializationResults
                {
                    RunID = _EXPORT_RUN_ID,
                    RecordCount = files.Count
                });

            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ID, _EXPORT_RUN_ID, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((int workspaceId, Guid runId, int batchSize, int start) =>
                {
                    try
                    {
                        return files.GetRange(start, batchSize)
                            .Select(f => new RelativityObjectSlim { ArtifactID = f.ArtifactId })
                            .ToArray();
                    }
                    catch (Exception)
                    {
                        return Array.Empty<RelativityObjectSlim>();
                    }
                });
        }

        private void VerifyCalculatedResultsWasSaved(List<File> files)
        {
            _rdoManagerMock.Verify(x => x.SetValuesAsync(_WORKSPACE_ID,
                It.Is<SyncStatisticsRdo>(s =>
                    s.RunId == _EXPORT_RUN_ID &&
                    s.CalculatedDocuments == files.Count &&
                    s.CalculatedFilesSize == files.Sum(f => f.Size) &&
                    s.CalculatedFilesCount == files.Count)));
        }

        private void VerifyBatchesWasRetrieved(int batchesCount)
        {
            _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(_WORKSPACE_ID, _EXPORT_RUN_ID,
                It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(batchesCount));
        }

        private void SetupSyncStatistics(SyncStatisticsRdo statistics)
        {
            _rdoManagerMock.Setup(x => x.GetAsync(_WORKSPACE_ID, It.IsAny<int>(),
                    It.IsAny<Expression<Func<SyncStatisticsRdo, object>>[]>()))
                .ReturnsAsync(statistics);
        }

        private List<File> PrepareFiles(int count)
        {
            List<File> files = new List<File>();
            for (int i = 1; i <= count; ++i)
            {
                files.Add(new File
                {
                    ArtifactId = i,
                    Size = i * 10
                });
            }

            return files;
        }

        private CompositeCancellationToken CreateDrainStopAlwaysCancellationToken()
        {
            return new CompositeCancellationTokenStub
            {
                IsDrainStopRequestedFunc = () => true
            };
        }

        private class File
        {
            public int ArtifactId { get; set; }

            public int Size { get; set; }
        }
    }
}
