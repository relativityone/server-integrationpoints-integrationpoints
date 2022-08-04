using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class DocumentAccumulatedStatisticsTests
    {

        private DocumentAccumulatedStatistics _sut;
        private Mock<IAPILog> _loggerFake;
        private Mock<IImageFileSizeStatistics> _imageFileSizeStatisticsFake;
        private Mock<INativeFileSizeStatistics> _nativeFileSizeStatisticsFake;
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private Mock<IExportQueryResult> _exportQueryResultFake;

        private Mock<IRelativityObjectManagerFactory> _objectManagerFactoryFake;
        private const int _WORKSPACE_ID = 1111;

        public IEnumerable<RelativityObjectSlim> Documents { get; set; }
        
        [SetUp]
        public void SetUp()
        {
            _objectManagerFake = new Mock<IRelativityObjectManager>();
            _objectManagerFactoryFake = new Mock<IRelativityObjectManagerFactory>();
            _objectManagerFactoryFake.Setup(x => x.CreateRelativityObjectManager(_WORKSPACE_ID))
                .Returns(_objectManagerFake.Object);
            _nativeFileSizeStatisticsFake = new Mock<INativeFileSizeStatistics>();
            _imageFileSizeStatisticsFake = new Mock<IImageFileSizeStatistics>();
            _loggerFake = new Mock<IAPILog>();
            _exportQueryResultFake = new Mock<IExportQueryResult>();

            _objectManagerFake.Setup(x =>
                    x.QueryWithExportAsync(It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(_exportQueryResultFake.Object);
            
            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .Returns((() => Task.FromResult(Documents)));

            _exportQueryResultFake.SetupGet(x => x.ExportResult)
                .Returns(() => new ExportInitializationResults
                    { FieldData = new List<FieldMetadata>(), RecordCount = Documents.Count(), RunID = Guid.NewGuid() });

            _sut = new DocumentAccumulatedStatistics(_objectManagerFactoryFake.Object, _nativeFileSizeStatisticsFake.Object, _imageFileSizeStatisticsFake.Object, _loggerFake.Object);
        }

        [Test]
        public async Task GetNativesStatisticsForSavedSearchAsync_ShouldCalculateStatistics()
        {
            // Arrange
            const int savedSearchArtifactId = 222;
            const int nativesSize = 33333;
            const int nativesCount = 2;

            List<RelativityObjectSlim> documents = Enumerable.Concat(
                Enumerable.Repeat(CreateDocumentWithHasNativeField(true), nativesCount),
                Enumerable.Repeat(CreateDocumentWithHasNativeField(false), 3)).ToList();

            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync(documents);
            
            _nativeFileSizeStatisticsFake.Setup(x => x.GetTotalFileSize(It.IsAny<IEnumerable<int>>(), _WORKSPACE_ID)).Returns(nativesSize);

            // Act
            DocumentsStatistics actual = await _sut.GetNativesStatisticsForSavedSearchAsync(_WORKSPACE_ID, savedSearchArtifactId).ConfigureAwait(false);

            // Assert
            actual.DocumentsCount.Should().Be(documents.Count);
            actual.TotalNativesCount.Should().Be(nativesCount);
            actual.TotalNativesSizeBytes.Should().Be(nativesSize);
        }
        
        [Test]
        public async Task GetImagesStatisticsForSavedSearchAsync_ShouldCalculateStatisticsWithSize()
        {
            // Arrange
            const int savedSearchArtifactId = 222;
            const int imagesSize = 33333;
            const int documentsWithImagesCount = 2;
            const int imagesPerDocumentCount = 5;

            Documents = Enumerable.Concat(
                Enumerable.Repeat(CreateDocumentWithImages(true, imagesPerDocumentCount), documentsWithImagesCount),
                Enumerable.Repeat(CreateDocumentWithImages(false, 0), 4)
                );

            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync(Documents);

            _imageFileSizeStatisticsFake.Setup(x => x.GetTotalFileSize(It.IsAny<IList<int>>(), _WORKSPACE_ID)).Returns(imagesSize);

            // Act
            DocumentsStatistics actual = await _sut.GetImagesStatisticsForSavedSearchAsync(_WORKSPACE_ID, savedSearchArtifactId, true).ConfigureAwait(false);

            // Assert
            actual.DocumentsCount.Should().Be(Documents.Count());
            actual.TotalImagesCount.Should().Be(documentsWithImagesCount * imagesPerDocumentCount);
            actual.TotalImagesSizeBytes.Should().Be(imagesSize);
        }


        [Test]
        public async Task GetImagesStatisticsForSavedSearchAsync_ShouldCalculateStatisticsWithoutSize()
        {
            // Arrange
            const int savedSearchArtifactId = 222;
            const int documentsWithImagesCount = 2;
            const int imagesPerDocumentCount = 5;

            Documents = Enumerable.Concat(
                Enumerable.Repeat(CreateDocumentWithImages(true, imagesPerDocumentCount), documentsWithImagesCount),
                Enumerable.Repeat(CreateDocumentWithImages(false, 0), 4)
            );

            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync(Documents);

            // Act
            DocumentsStatistics actual = await _sut.GetImagesStatisticsForSavedSearchAsync(_WORKSPACE_ID, savedSearchArtifactId, false).ConfigureAwait(false);

            // Assert
            actual.DocumentsCount.Should().Be(Documents.Count());
            actual.TotalImagesCount.Should().Be(documentsWithImagesCount * imagesPerDocumentCount);
            actual.TotalImagesSizeBytes.Should().Be(0);
        }

        [Test]
        public async Task GetImagesStatisticsForProductionAsync_ShouldCalculateStatisticsWithSize()
        {
            // Arrange
            const int productionArtifactId = 222;
            const int imagesSize = 33333;
            const int documentsWithImagesCount = 2;
            const int imagesPerDocumentCount = 5;

            Documents = Enumerable.Concat(
                Enumerable.Repeat(CreateDocumentWithProducedImages(imagesPerDocumentCount), documentsWithImagesCount),
                Enumerable.Repeat(CreateDocumentWithProducedImages(0), 4)
            ).ToList();

            _exportQueryResultFake.Setup(x => x.GetNextBlockAsync(0, It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync(Documents);
            _imageFileSizeStatisticsFake.Setup(x => x.GetTotalFileSize(productionArtifactId, _WORKSPACE_ID)).Returns(imagesSize);

            // Act
            DocumentsStatistics actual = await _sut.GetImagesStatisticsForProductionAsync(_WORKSPACE_ID, productionArtifactId).ConfigureAwait(false);

            // Assert
            actual.DocumentsCount.Should().Be(Documents.Count());
            actual.TotalImagesCount.Should().Be(documentsWithImagesCount * imagesPerDocumentCount);
            actual.TotalImagesSizeBytes.Should().Be(imagesSize);
        }
        
        private RelativityObjectSlim CreateDocumentWithHasNativeField(bool hasNative)
        {
            return new RelativityObjectSlim()
            {
                Values = new List<object>
                {
                    hasNative
                }
            };
        }

        private RelativityObjectSlim CreateDocumentWithImages(bool hasImages, int imagesCount)
        {
            return new RelativityObjectSlim
            {
                Values = new List<object>
                {
                    new Choice()
                    {
                        Name = hasImages ? "Yes" : "No"
                    },
                    imagesCount
                }
            };
        }

        private RelativityObjectSlim CreateDocumentWithProducedImages(int imagesCount)
        {
            return new RelativityObjectSlim
            {
                Values = new List<object>
                {
                    imagesCount
                }
            };
        }
    }
}