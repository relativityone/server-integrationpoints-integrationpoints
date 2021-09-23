using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
	[TestFixture, Category("Unit")]
	public class DocumentAccumulatedStatisticsTests
	{
		private const int _WORKSPACE_ID = 1111;

		private readonly Guid _hasNativeFieldGuid = new Guid("E09E18F3-D0C8-4CFC-96D1-FBB350FAB3E1");
		private readonly Guid _relativityImageCountGuid = new Guid("D726B2D9-4192-43DF-86EF-27D36560931A");
		private readonly Guid _productionImageCountFieldGuid = new Guid("D92B5B06-CDF0-44BA-B365-A2396F009C73");
		private readonly string _hasImagesFieldName = "Has Images";

		private Mock<IRelativityObjectManagerFactory> _objectManagerFactoryFake;
		private Mock<IRelativityObjectManager> _objectManagerFake;
		private Mock<INativeFileSizeStatistics> _nativeFileSizeStatisticsFake;
		private Mock<IImageFileSizeStatistics> _imageFileSizeStatisticsFake;
		private Mock<IAPILog> _loggerFake;

		private DocumentAccumulatedStatistics _sut;

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

			_sut = new DocumentAccumulatedStatistics(_objectManagerFactoryFake.Object, _nativeFileSizeStatisticsFake.Object, _imageFileSizeStatisticsFake.Object, _loggerFake.Object);
		}

		[Test]
		public async Task GetNativesStatisticsForSavedSearchAsync_ShouldCalculateStatistics()
		{
			// Arrange
			const int savedSearchArtifactId = 222;
			const int nativesSize = 33333;
			const int nativesCount = 2;

			List<RelativityObject> documents = Enumerable.Concat(
				Enumerable.Repeat(CreateDocumentWithHasNativeField(true), nativesCount),
				Enumerable.Repeat(CreateDocumentWithHasNativeField(false), 3)).ToList();

			SetupObjectManagerForNatives(savedSearchArtifactId, documents);
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

			List<RelativityObject> documents = Enumerable.Concat(
				Enumerable.Repeat(CreateDocumentWithImages(true, imagesPerDocumentCount), documentsWithImagesCount),
				Enumerable.Repeat(CreateDocumentWithImages(false, 0), 4)
				).ToList();

			SetupObjectManagerForImages(savedSearchArtifactId, documents);
			_imageFileSizeStatisticsFake.Setup(x => x.GetTotalFileSize(It.IsAny<IList<int>>(), _WORKSPACE_ID)).Returns(imagesSize);

			// Act
			DocumentsStatistics actual = await _sut.GetImagesStatisticsForSavedSearchAsync(_WORKSPACE_ID, savedSearchArtifactId, true).ConfigureAwait(false);

			// Assert
			actual.DocumentsCount.Should().Be(documents.Count);
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

			List<RelativityObject> documents = Enumerable.Concat(
				Enumerable.Repeat(CreateDocumentWithImages(true, imagesPerDocumentCount), documentsWithImagesCount),
				Enumerable.Repeat(CreateDocumentWithImages(false, 0), 4)
			).ToList();

			SetupObjectManagerForImages(savedSearchArtifactId, documents);

			// Act
			DocumentsStatistics actual = await _sut.GetImagesStatisticsForSavedSearchAsync(_WORKSPACE_ID, savedSearchArtifactId, false).ConfigureAwait(false);

			// Assert
			actual.DocumentsCount.Should().Be(documents.Count);
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

			List<RelativityObject> documents = Enumerable.Concat(
				Enumerable.Repeat(CreateDocumentWithProducedImages(imagesPerDocumentCount), documentsWithImagesCount),
				Enumerable.Repeat(CreateDocumentWithProducedImages(0), 4)
			).ToList();

			SetupObjectManagerForProducedImages(productionArtifactId, documents);
			_imageFileSizeStatisticsFake.Setup(x => x.GetTotalFileSize(productionArtifactId, _WORKSPACE_ID)).Returns(imagesSize);

			// Act
			DocumentsStatistics actual = await _sut.GetImagesStatisticsForProductionAsync(_WORKSPACE_ID, productionArtifactId).ConfigureAwait(false);

			// Assert
			actual.DocumentsCount.Should().Be(documents.Count);
			actual.TotalImagesCount.Should().Be(documentsWithImagesCount * imagesPerDocumentCount);
			actual.TotalImagesSizeBytes.Should().Be(imagesSize);
		}
		
		private RelativityObject CreateDocumentWithHasNativeField(bool hasNative)
		{
			return new RelativityObject()
			{
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Guids = new List<Guid>()
							{
								_hasNativeFieldGuid
							}
						},
						Value = hasNative
					}
				}
			};
		}

		private RelativityObject CreateDocumentWithImages(bool hasImages, int imagesCount)
		{
			return new RelativityObject()
			{
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = _hasImagesFieldName
						},
						Value = new Choice()
						{
							Name = hasImages ? "Yes" : "No"
						}
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Relativity Image Count",
							Guids = new List<Guid>()
							{
								_relativityImageCountGuid
							}
						},
						Value = imagesCount
					}
				}
			};
		}

		private RelativityObject CreateDocumentWithProducedImages(int imagesCount)
		{
			return new RelativityObject()
			{
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Guids = new List<Guid>()
							{
								_productionImageCountFieldGuid
							}
						},
						Value = imagesCount
					}
				}
			};
		}

		private void SetupObjectManagerForNatives(int savedSearchArtifactId, List<RelativityObject> documents)
		{
			_objectManagerFake
				.Setup(x => x.Query(It.Is<QueryRequest>(request =>
						request.Condition.Equals($"'ArtifactId' IN SAVEDSEARCH {savedSearchArtifactId}", StringComparison.InvariantCultureIgnoreCase) &&
						request.Fields.Single().Guid == _hasNativeFieldGuid),
					ExecutionIdentity.CurrentUser))
				.Returns(documents);
		}

		private void SetupObjectManagerForImages(int savedSearchArtifactId, List<RelativityObject> documents)
		{
			_objectManagerFake
				.Setup(x => x.Query(It.Is<QueryRequest>(request =>
						request.Condition.Equals($"'ArtifactId' IN SAVEDSEARCH {savedSearchArtifactId}", StringComparison.InvariantCultureIgnoreCase) &&
						request.Fields.Any(field => field.Guid == _relativityImageCountGuid) &&
						request.Fields.Any(field => field.Name == _hasImagesFieldName)),
					ExecutionIdentity.CurrentUser))
				.Returns(documents);
		}

		private void SetupObjectManagerForProducedImages(int productionArtifactId, List<RelativityObject> documents)
		{
			_objectManagerFake
				.Setup(x => x.Query(It.Is<QueryRequest>(request =>
						request.Condition.Equals($"'ProductionSet' == OBJECT {productionArtifactId}", StringComparison.InvariantCultureIgnoreCase) &&
						request.Fields.Any(field => field.Guid == _productionImageCountFieldGuid)),
					ExecutionIdentity.CurrentUser))
				.Returns(documents);
		}

	}
}