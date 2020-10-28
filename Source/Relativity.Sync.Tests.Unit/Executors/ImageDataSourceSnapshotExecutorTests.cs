using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class ImageDataSourceSnapshotExecutorTests
	{
		private ImageDataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;
		private Mock<IImageDataSourceSnapshotConfiguration> _configurationMock;
		private Mock<IJobProgressUpdater> _jobProgressUpdater;
		private Mock<IImageFileRepository> _imageFileRepositoryMock;
		private IJobStatisticsContainer _jobStatisticsContainer;
		private Mock<IFieldManager> _fieldManager;

		private const int _WORKSPACE_ID = 458712;
		private const int _DATA_SOURCE_ID = 485219;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_fieldManager = new Mock<IFieldManager>();
			_fieldManager.Setup(fm => fm.GetDocumentTypeFieldsAsync(CancellationToken.None)).ReturnsAsync(Mock.Of<List<FieldInfoDto>>());
			_fieldManager.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(new FieldInfoDto(SpecialFieldType.None, "a", "a", true, true));


			_configurationMock = new Mock<IImageDataSourceSnapshotConfiguration>();
			_configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			_configurationMock.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);

			_imageFileRepositoryMock = new Mock<IImageFileRepository>();
			_jobStatisticsContainer = new JobStatisticsContainer();

			_jobProgressUpdater = new Mock<IJobProgressUpdater>();
			Mock<IJobProgressUpdaterFactory> jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
			jobProgressUpdaterFactory.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdater.Object);

			_instance = new ImageDataSourceSnapshotExecutor(serviceFactory.Object,
				_imageFileRepositoryMock.Object, _jobStatisticsContainer, _fieldManager.Object, new EmptyLogger());
		}

		[Test]
		public async Task ExecuteAsync_ShouldSetImagesSize()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			// Arrange
			ImagesStatistics expectedImagesStatistics = new ImagesStatistics(5, 50);

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults();

			_imageFileRepositoryMock
				.Setup(x => x.CalculateImagesStatisticsAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>()))
				.Returns(Task.FromResult(expectedImagesStatistics));

			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// Act
			await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);
			ImagesStatistics imagesStatistics = await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false);

			// Assert
			imagesStatistics.Should().Be(expectedImagesStatistics);
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[Test]
		public async Task ExecuteAsync_ShouldInitializeExportAndSaveResult_WhenPushingImagesWithoutProductionPrecedence()
		{
			const int totalRecords = 123456789;
			Guid runId = Guid.NewGuid();

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);
			_objectManager.Setup(x => x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(Array.Empty<RelativityObjectSlim>());
			_imageFileRepositoryMock.Setup(x => x.QueryImagesForDocumentsAsync(It.IsAny<int>(), It.IsAny<int[]>(), It.IsAny<QueryImagesOptions>())).ReturnsAsync(Enumerable.Empty<ImageFile>());

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequestForImagesWithoutProductionPrecedence(qr)), 1));
			_configurationMock.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
			_jobProgressUpdater.Verify(x => x.SetTotalItemsCountAsync(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsync_ShouldBuildValidQueryForExportAPI_WhenPushingImagesWithProductionPrecedence()
		{
			const int totalRecords = 123;
			const int productionId = 1111;
			Guid runId = Guid.NewGuid();

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};
			_configurationMock.SetupGet(x => x.ProductionImagePrecedence).Returns(new int[] { productionId });
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequestForImagesWithProductionPrecedence(qr)), 1));
		}

		[Test]
		public async Task ExecuteAsync_ShouldFailWhenExportApiFails()
		{
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

			// ACT
			ExecutionResult executionResult = await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
			executionResult.Exception.Should().BeOfType<InvalidOperationException>();
		}

		private bool AssertQueryRequestForImagesWithoutProductionPrecedence(QueryRequest queryRequest)
		{
			AssertObjectType(queryRequest);
			queryRequest.Condition.Should().Be($"('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND ('Has Images' == CHOICE 1034243)");
			return true;
		}

		private bool AssertQueryRequestForImagesWithProductionPrecedence(QueryRequest queryRequest)
		{
			AssertObjectType(queryRequest);
			queryRequest.Condition.Should().Be($"('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND ('Production::Image Count' > 0)");
			return true;
		}

		private void AssertObjectType(QueryRequest request)
		{
			const int documentArtifactTypeId = (int)ArtifactType.Document;
			request.ObjectType.ArtifactTypeID.Should().Be(documentArtifactTypeId);
		}
	}
}