using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    internal class FileStatisticsCalculatorTests : SystemTest
    {
        private WorkspaceRef _workspace;

        private readonly Dataset ImageDataSet = Dataset.ThreeImages;
        private readonly Dataset NativeDataSet = Dataset.NativesAndExtractedText;

        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            _workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

            ImportDataTableWrapper imageDataTableWrapper = DataTableFactory.CreateImageImportDataTable(ImageDataSet);
            await ImportHelper.ImportDataAsync(_workspace.ArtifactID, imageDataTableWrapper).ConfigureAwait(false);

            ImportDataTableWrapper nativeDataTableWrapper = DataTableFactory.CreateImportDataTable(NativeDataSet, false, true);
            await ImportHelper.ImportDataAsync(_workspace.ArtifactID, nativeDataTableWrapper).ConfigureAwait(false);
        }

        [IdentifiedTest("15873C0E-69A7-4497-B319-7E3B2A85C552")]
        public async Task CalculateImagesTotalSizeAsync_ShouldCalculateImagesFileSize()
        {
            // Arrange
            QueryRequest request = GetImageQueryRequest();

            IFileStatisticsCalculator sut = await PrepareSut().ConfigureAwait(false);

            // Act
            ImagesStatistics calculatedImagesStatistics = await sut.CalculateImagesStatisticsAsync(_workspace.ArtifactID, request,
                new QueryImagesOptions(), CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            calculatedImagesStatistics.TotalCount.Should().Be(ImageDataSet.TotalItemCount);
            calculatedImagesStatistics.TotalSize.Should().Be(ImageDataSet.GetTotalFilesSize());
        }

        [IdentifiedTest("779ABAE7-85DC-4CC1-82F8-D1E5048C3C1E")]
        public async Task CalculateImagesTotalSizeAsync_ShouldSaveResultsInSyncStatistics_WhenDrainStopWasTriggered()
        {
            // Arrange
            const int batchSize = 2;

            QueryRequest request = GetImageQueryRequest();

            TokenDrainStoppingAfterProcessingFirstBatch cancellationToken =
                new TokenDrainStoppingAfterProcessingFirstBatch();

            ConfigurationStub configuration = new ConfigurationStub
            {
                BatchSizeForFileQueries = batchSize
            };

            IContainer container = await PrepareContainer(configuration).ConfigureAwait(false);

            IFileStatisticsCalculator sut = container.Resolve<IFileStatisticsCalculator>();

            // Act
            ImagesStatistics calculatedImagesStatistics = await sut.CalculateImagesStatisticsAsync(_workspace.ArtifactID, request,
                new QueryImagesOptions(), cancellationToken).ConfigureAwait(false);

            // Assert
            calculatedImagesStatistics.TotalCount.Should().Be(batchSize);
            calculatedImagesStatistics.TotalSize.Should().BePositive();

            int expectedCalculatedDocumentsCount = batchSize;
            await AssertSyncStatistics(container, configuration.SyncStatisticsId,
                ImageDataSet, expectedCalculatedDocumentsCount).ConfigureAwait(false);
        }

        [IdentifiedTest("4C7D7E45-4830-47BC-BB07-DEB4511A05D5")]
        public async Task CalculateNativesTotalSizeAsync_ShouldCalculateNativesFileSize()
        {
            // Arrange
            QueryRequest request = GetNativesQueryRequest();

            IFileStatisticsCalculator sut = await PrepareSut().ConfigureAwait(false);

            // Act
            long calculatedNativesSize = await sut.CalculateNativesTotalSizeAsync(_workspace.ArtifactID, request,
                CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            calculatedNativesSize.Should().Be(NativeDataSet.GetTotalFilesSize("NATIVES"));
        }

        [IdentifiedTest("8D6522F8-EDE2-4FDD-A155-C98420004CAB")]
        public async Task CalculateNativesTotalSizeAsync_ShouldSaveResultsInSyncStatistics_WhenDrainStopWasTriggered()
        {
            // Arrange
            const int batchSize = 2;

            QueryRequest request = GetNativesQueryRequest();

            TokenDrainStoppingAfterProcessingFirstBatch cancellationToken =
                new TokenDrainStoppingAfterProcessingFirstBatch();

            ConfigurationStub configuration = new ConfigurationStub
            {
                BatchSizeForFileQueries = batchSize
            };

            IContainer container = await PrepareContainer(configuration).ConfigureAwait(false);

            IFileStatisticsCalculator sut = container.Resolve<IFileStatisticsCalculator>();

            // Act
            long calculatedNativesSize = await sut.CalculateNativesTotalSizeAsync(_workspace.ArtifactID, request,
                cancellationToken).ConfigureAwait(false);

            // Assert
            calculatedNativesSize.Should().BePositive();

            int expectedCalculatedDocumentsCount = batchSize;
            await AssertSyncStatistics(container, configuration.SyncStatisticsId,
                NativeDataSet, expectedCalculatedDocumentsCount).ConfigureAwait(false);
        }

        [IdentifiedTest("E5F9E2DF-117B-45DF-949B-4ABCCB0BDCFE")]
        public async Task CalculateNativesTotalSizeAsync_ResumingNativeSizeCalculation()
        {
            // Arrange
            const int batchSize = 2;

            QueryRequest request = GetNativesQueryRequest();

            TokenDrainStoppingAfterProcessingFirstBatch cancellationToken = 
                new TokenDrainStoppingAfterProcessingFirstBatch();

            ConfigurationStub configuration = new ConfigurationStub
            {
                BatchSizeForFileQueries = batchSize
            };

            IContainer container = await PrepareContainer(configuration).ConfigureAwait(false);

            IFileStatisticsCalculator sut = container.Resolve<IFileStatisticsCalculator>();

            // Act & Assert Drain-Stopping
            long calculatedNativesSize = await sut.CalculateNativesTotalSizeAsync(_workspace.ArtifactID, request,
                cancellationToken).ConfigureAwait(false);

            calculatedNativesSize.Should().BePositive();

            int expectedCalculatedDocumentsCount = batchSize;
            await AssertSyncStatistics(container, configuration.SyncStatisticsId,
                NativeDataSet, expectedCalculatedDocumentsCount).ConfigureAwait(false);

            // Act & Assert Resuming
            calculatedNativesSize = await sut.CalculateNativesTotalSizeAsync(_workspace.ArtifactID, request,
                cancellationToken).ConfigureAwait(false);

            calculatedNativesSize.Should().Be(NativeDataSet.GetTotalFilesSize("NATIVES"));

            expectedCalculatedDocumentsCount = NativeDataSet.TotalDocumentCount;
            await AssertSyncStatistics(container, configuration.SyncStatisticsId,
                NativeDataSet, expectedCalculatedDocumentsCount).ConfigureAwait(false);

            // Resuming should build remaining batches only from all the other documents
            int expectedBatchesCount = (int)Math.Ceiling((double)expectedCalculatedDocumentsCount / batchSize);
            cancellationToken.ChecksCount.Should().Be(expectedBatchesCount);
        }

        private async Task<IFileStatisticsCalculator> PrepareSut()
        {
            ConfigurationStub configuration = new ConfigurationStub();

            IContainer container = await PrepareContainer(configuration).ConfigureAwait(false);

            return container.Resolve<IFileStatisticsCalculator>();
        }

        private async Task<IContainer> PrepareContainer(ConfigurationStub configuration)
        {
            int syncStatisticsId = await Rdos.CreateEmptySyncStatisticsRdoAsync(_workspace.ArtifactID).ConfigureAwait(false);

            configuration.SyncStatisticsId = syncStatisticsId;

            return ContainerHelper.Create(configuration);
        }

        private async Task AssertSyncStatistics(IContainer container, int syncStatisticsId,
            Dataset dataSet, int expectedCalculatedDocumentsCount)
        {
            IRdoManager rdoManager = container.Resolve<IRdoManager>();

            SyncStatisticsRdo syncStatistics = await rdoManager.GetAsync<SyncStatisticsRdo>(
                _workspace.ArtifactID, syncStatisticsId).ConfigureAwait(false);

            syncStatistics.RunId.Should().NotBeEmpty();
            syncStatistics.RequestedDocuments.Should().Be(dataSet.TotalDocumentCount);
            syncStatistics.CalculatedDocuments.Should().Be(expectedCalculatedDocumentsCount);
            syncStatistics.CalculatedFilesCount.Should().Be(expectedCalculatedDocumentsCount);
            syncStatistics.CalculatedFilesSize.Should().BePositive();
        }

        private QueryRequest GetImageQueryRequest()
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = "'Relativity Image Count' > 0"
            };
            return request;
        }

        private QueryRequest GetNativesQueryRequest()
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
                },
                Condition = "'Has Native' == true"
            };
            return request;
        }

        private class TokenDrainStoppingAfterProcessingFirstBatch : CompositeCancellationTokenStub
        {
            public int ChecksCount { get; private set; }

            public TokenDrainStoppingAfterProcessingFirstBatch()
            {
                IsDrainStopRequestedFunc = ShouldDrainStopOnceOnFirstCall;
            }

            private bool ShouldDrainStopOnceOnFirstCall()
            {
                bool shouldDrainStop = ChecksCount == 0;
                ++ChecksCount;

                return shouldDrainStop;
            }
        }
    }
}
