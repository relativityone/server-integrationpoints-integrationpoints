using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using kCura.EDDS.WebAPI.FileManagerBase;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class ImageFileRepositoryTests
	{
		private const int WORKSPACE_ID = 5;

		private Mock<ISearchManagerFactory> _searchManagerFactoryMock;
		private Mock<ISyncLog> _loggerMock;
		private ImageFileRepository _sut;
		private Mock<ISearchManager> _searchManagerMock;

		[SetUp]
		public void Setup()
		{
			_searchManagerFactoryMock = new Mock<ISearchManagerFactory>();
			_loggerMock = new Mock<ISyncLog>();

			_searchManagerMock = new Mock<ISearchManager>();

			_searchManagerFactoryMock.Setup(x => x.CreateSearchManagerAsync()).ReturnsAsync(_searchManagerMock.Object);


			_sut = new ImageFileRepository(_searchManagerFactoryMock.Object, _loggerMock.Object);
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnDocumentImages()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x }).ToList();

			_searchManagerMock.Setup(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()))
				.Returns(CreateDataSet(data));

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions()).ConfigureAwait(false);

			// Assert
			result.Select(x => x.DocumentArtifactId).Should().BeEquivalentTo(data.Select(x => x.DocumentArtifactId));
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnDocumentImages_And_SkipDocumentsWithoutImages()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x }).ToList();

			_searchManagerMock.Setup(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()))
				.Returns(CreateDataSet(data));

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).Concat(Enumerable.Range(20, 5)).ToArray(),
				new QueryImagesOptions()).ConfigureAwait(false);

			// Assert
			result.Select(x => x.DocumentArtifactId).Should().BeEquivalentTo(data.Select(x => x.DocumentArtifactId));
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnMultipleImagesPerDocument()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x % 2 }).ToList();

			_searchManagerMock.Setup(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()))
				.Returns(CreateDataSet(data));

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions()).ConfigureAwait(false);

			// Assert
			result.Select(x => x.DocumentArtifactId).Should().BeEquivalentTo(data.Select(x => x.DocumentArtifactId));
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnProducedDocumentImages()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 1 }).ToList();


			MockProductions(data);

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions { ProductionIds = new[] { 1 } }).ConfigureAwait(false);

			// Assert
			result.Select(x => x.DocumentArtifactId).Should().BeEquivalentTo(data.Select(x => x.DocumentArtifactId));
			result.All(x => x.ProductionId == 1).Should().BeTrue();
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnProducedDocumentImages_And_IncludeOriginalImages()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = (x % 2) + 1 }).ToList();

			MockProductions(data);

			_searchManagerMock.Setup(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()))
				.Returns(CreateDataSet(data.Where(x => x.ProductionId == 2)));

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions { ProductionIds = new[] { 1 }, IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			data.All(x => result.Any(r => r.DocumentArtifactId == x.DocumentArtifactId)).Should().BeTrue();

			result.Count(x => x.ProductionId == 1).Should().Be(5);
			result.Count(x => x.ProductionId == null).Should().Be(5);
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldNotCallOriginalImages_WhenIncludeOriginalImagesIsSetAndAllImagesMeetProductionPrecedence()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 1 }).ToList();

			MockProductions(data);

			QueryImagesOptions options = new QueryImagesOptions
				{ ProductionIds = new[] {1}, IncludeOriginalImageIfNotFoundInProductions = true };

			int[] documentIds = data.Select(x => x.DocumentArtifactId).ToArray();

			// Act
			await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, documentIds, options).ConfigureAwait(false);

			// Assert
			_searchManagerMock.Verify(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()), Times.Never);
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldNotGetOriginalImages_WhenIncludeOriginalImagesIsSetAndImageForProductionWasFound()
		{
			// Arrange
			const int productionId = 1;

			var productionData = new DocumentImageData[]
			{
				new DocumentImageData { DocumentArtifactId = 1000, ProductionId = productionId },
				new DocumentImageData { DocumentArtifactId = 1001, ProductionId = 2 }
			};

			MockProductions(productionData);

			_searchManagerMock.Setup(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, It.IsAny<int[]>()))
				.Returns(CreateDataSet(productionData.Where(x => x.ProductionId == 1)));

			var documentIdsWithProductionId = productionData
				.Where(x => x.ProductionId == productionId)
				.Select(x => x.DocumentArtifactId);

			QueryImagesOptions options = new QueryImagesOptions
				{ ProductionIds = new[] { productionId }, IncludeOriginalImageIfNotFoundInProductions = true };

			int[] documentIds = productionData.Select(x => x.DocumentArtifactId).ToArray();

			// Act
			await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, documentIds, options).ConfigureAwait(false);

			// Assert
			_searchManagerMock.Verify(x => x.RetrieveImagesForDocuments(WORKSPACE_ID, 
				It.Is<int[]>(expectedIds => expectedIds.Intersect(documentIdsWithProductionId).Any())), Times.Never);
		}


		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldReturnProducedDocumentImages_WithRespectToProductionPrecedence()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 1 })
				.Concat(Enumerable.Range(1, 5)
					.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 2 }))
				.ToList();

			MockProductions(data);

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions { ProductionIds = new[] { 2, 1 } }).ConfigureAwait(false);

			// Assert
			result.Select(x => x.DocumentArtifactId).Should().BeEquivalentTo(Enumerable.Range(1, 10));


			result.Where(x => x.ProductionId == 1).Select(x => x.DocumentArtifactId).Should()
				.BeEquivalentTo(Enumerable.Range(6, 5));

			result.Where(x => x.ProductionId == 2).Select(x => x.DocumentArtifactId).Should()
				.BeEquivalentTo(Enumerable.Range(1, 5));
		}

		[Test]
		public async Task QueryImagesForDocumentsAsync_ShouldStopSearchForProducedDocumentImages_WhenAllImagesFound()
		{
			// Arrange
			var data = Enumerable.Range(1, 10)
				.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 1 })
				.Concat(Enumerable.Range(1, 5)
					.Select(x => new DocumentImageData { DocumentArtifactId = x, ProductionId = 2 }))
				.ToList();

			MockProductions(data);
			

			// Act
			IEnumerable<ImageFile> result = await _sut.QueryImagesForDocumentsAsync(WORKSPACE_ID, data.Select(x => x.DocumentArtifactId).ToArray(),
				new QueryImagesOptions { ProductionIds = new[] { 1, 2 } }).ConfigureAwait(false);

			// Assert
			_searchManagerMock.Verify(x => x.RetrieveImagesForProductionDocuments(WORKSPACE_ID, It.IsAny<int[]>(),1), Times.Once);
			_searchManagerMock.Verify(x => x.RetrieveImagesForProductionDocuments(WORKSPACE_ID, It.IsAny<int[]>(),2), Times.Never);
		}


		private void MockProductions(IEnumerable<DocumentImageData> data)
		{
			foreach (var productionSet in data.GroupBy(x => x.ProductionId).Where(x => x.Key != null))
			{
				_searchManagerMock
					.Setup(x => x.RetrieveImagesForProductionDocuments(WORKSPACE_ID, It.IsAny<int[]>(),
						productionSet.Key.Value)).Returns(CreateDataSet(productionSet));
			}
		}

		private DataSet CreateDataSet(IEnumerable<DocumentImageData> data)
		{
			var dataTable = new DataTable();

			dataTable.Columns.Add("DocumentArtifactID", typeof(int));
			dataTable.Columns.Add("Location");
			dataTable.Columns.Add("ImageFileName");
			dataTable.Columns.Add("ImageSize", typeof(long));
			dataTable.Columns.Add("Filename");
			dataTable.Columns.Add("NativeIdentifier");
			dataTable.Columns.Add("Identifier");
			dataTable.Columns.Add("Size", typeof(long));

			foreach (var imageData in data)
			{
				DataRow dataRow = dataTable.NewRow();

				dataRow["DocumentArtifactID"] = imageData.DocumentArtifactId;
				dataRow["NativeIdentifier"] = imageData.DocumentArtifactId.ToString();
				dataRow["Identifier"] = imageData.Identifier ?? (imageData.DocumentArtifactId.ToString() + (imageData.ProductionId != null ? "_" + imageData.ProductionId : ""));
				dataRow["Location"] = "location";
				dataRow["ImageFileName"] = imageData.DocumentArtifactId.ToString();
				dataRow["Filename"] = imageData.DocumentArtifactId.ToString();
				dataRow["Size"] = imageData.ImageSize;
				dataRow["ImageSize"] = imageData.ImageSize;

				dataTable.Rows.Add(dataRow);
			}

			return new DataSet { Tables = { dataTable } };
		}

		private struct DocumentImageData
		{
			public int DocumentArtifactId { get; set; }
			public string Identifier { get; set; }
			public int ImageSize { get; set; }
			public int? ProductionId { get; set; }
		}
	}
}
