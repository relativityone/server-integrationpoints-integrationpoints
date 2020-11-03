using System;
using System.Collections.Generic;
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
	internal sealed class ImageRetryDataSourceSnapshotExecutorTests
	{
		private ImageRetryDataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;
		private Mock<IImageRetryDataSourceSnapshotConfiguration> _configurationMock;
		private Mock<IImageFileRepository> _imageFileRepositoryMock;
		private IJobStatisticsContainer _jobStatisticsContainer;
		private Mock<IFieldManager> _fieldManager;

		private readonly FieldInfoDto _IDENTIFIER_FIELD = new FieldInfoDto(SpecialFieldType.None, "Control Number", "Control Number", true, true);

		private const int _WORKSPACE_ID = 458712;
		private const int _DATA_SOURCE_ID = 485219;
		private const int _RETRY_JOB_HISTORY_ID = 10;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_fieldManager = new Mock<IFieldManager>();
			_fieldManager.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(_IDENTIFIER_FIELD);

			_configurationMock = new Mock<IImageRetryDataSourceSnapshotConfiguration>();
			_configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			_configurationMock.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);

			_imageFileRepositoryMock = new Mock<IImageFileRepository>();
			_jobStatisticsContainer = new JobStatisticsContainer();

			_instance = new ImageRetryDataSourceSnapshotExecutor(serviceFactory.Object,
				_imageFileRepositoryMock.Object, _jobStatisticsContainer, _fieldManager.Object, new EmptyLogger());
		}

		[Test]
		public async Task ExecuteAsync_ShouldCalculateImageSize()
		{
			// Arrange
			ImagesStatistics expectedImagesStatistics = new ImagesStatistics(It.IsAny<int>(), It.IsAny<int>());
			_imageFileRepositoryMock
				.Setup(x => x.CalculateImagesStatisticsAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<QueryImagesOptions>()))
				.Returns(Task.FromResult(expectedImagesStatistics));

			SetupExportInitialization();

			// Act
			await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);
			ImagesStatistics imagesStatistics = await _jobStatisticsContainer.ImagesStatistics.ConfigureAwait(false);

			// Assert
			imagesStatistics.Should().Be(expectedImagesStatistics);
		}

		[Test]
		public async Task ExecuteAsync_ShouldSaveResults()
		{
			// Arrange
			const int expectedTotalRecords = 10;
			Guid expectedRunId = Guid.NewGuid();

			SetupExportInitialization(expectedRunId, expectedTotalRecords);

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			_configurationMock.Verify(x => x.SetSnapshotDataAsync(expectedRunId, expectedTotalRecords));
		}

		public static IEnumerable<TestCaseData> ImageInformationTestCaseSourceData => new[]
		{
			new TestCaseData(new [] { 1 }, false,
				$"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_RETRY_JOB_HISTORY_ID}])) AND ('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND ('Production::Image Count' > 0)"),
			new TestCaseData(new [] { 1 }, true,
				$"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_RETRY_JOB_HISTORY_ID}])) AND ('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND (('Production::Image Count' > 0) OR ('Has Images' == CHOICE 1034243))"),
			new TestCaseData(new int[] { }, true,
				$"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_RETRY_JOB_HISTORY_ID}])) AND ('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND ('Has Images' == CHOICE 1034243)"),
			new TestCaseData(new int[] { }, false,
				$"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{_RETRY_JOB_HISTORY_ID}])) AND ('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}) AND ('Has Images' == CHOICE 1034243)")
		};

		[TestCaseSource(nameof(ImageInformationTestCaseSourceData))]
		public async Task ExecuteAsync_ShouldBuildValidQueryWithJobHistoryErrors_WhenJobHistoryIdIsSet(int[] productionImagePrecedence,
			bool includeOriginalImageIfNotFoundInProductions, string expectedQueryRequestCondition)
		{
			// Arrange
			_configurationMock.SetupGet(x => x.ProductionImagePrecedence)
				.Returns(productionImagePrecedence);
			_configurationMock.SetupGet(x => x.IncludeOriginalImageIfNotFoundInProductions)
				.Returns(includeOriginalImageIfNotFoundInProductions);
			_configurationMock.SetupGet(x => x.JobHistoryToRetryId)
				.Returns(_RETRY_JOB_HISTORY_ID);

			SetupExportInitialization();

			// Act
			await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID,
				It.Is<QueryRequest>(qr => AssertQueryRequest(qr, expectedQueryRequestCondition)), 1));
		}

		[Test]
		public async Task ExecuteAsync_ShouldFailWhenExportApiFails()
		{
			// Arrange
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

			// Act
			ExecutionResult executionResult = await _instance.ExecuteAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
			executionResult.Exception.Should().BeOfType<InvalidOperationException>();
		}

		private void SetupExportInitialization(Guid runId, int totalRecords)
		{
			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};

			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);
		}

		private void SetupExportInitialization()
		{
			SetupExportInitialization(Guid.Empty, 0);
		}

		private bool AssertQueryRequest(QueryRequest queryRequest, string expectedQueryRequestCondition)
		{
			queryRequest.ObjectType.ArtifactTypeID.Should().Be((int)ArtifactType.Document);
			queryRequest.Fields.Should().ContainSingle(f => f.Name == _IDENTIFIER_FIELD.SourceFieldName);
			queryRequest.Condition.Should().Be(expectedQueryRequestCondition);

			return true;
		}
	}
}