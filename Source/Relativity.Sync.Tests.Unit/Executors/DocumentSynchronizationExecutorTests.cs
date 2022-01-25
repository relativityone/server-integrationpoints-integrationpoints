using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Transfer;
using IStopwatch = Relativity.Sync.Utils.IStopwatch;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal class DocumentSynchronizationExecutorTests
	{
		private Mock<IBatchRepository> _batchRepositoryMock;
		private Mock<IFieldManager> _fieldManagerFake;
		private Mock<IFieldMappings> _fakeFieldMappings;
		private Mock<IJobStatisticsContainer> _jobStatisticsContainerFake;
		private Mock<IImportJobFactory> _importJobFactoryFake;
		private Mock<IJobCleanupConfiguration> _jobCleanupConfigurationMock;
		private Mock<IJobProgressHandlerFactory> _jobProgressHandlerFactoryStub;
		private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactoryStub;
		private Mock<IJobProgressHandler> _jobProgressHandlerFake;
		private Mock<IJobProgressUpdater> _jobProgressUpdaterFake;
		private Mock<ITaggingProvider> _taggingProviderFake;
		private Mock<IAutomatedWorkflowTriggerConfiguration> _automatedWorkflowTriggerConfigurationFake;
		private Mock<Func<IStopwatch>> _stopwatchFactoryFake;
		private Mock<IStopwatch> _stopwatchFake;
		private Mock<ISyncMetrics> _syncMetricsMock;

		private Mock<Sync.Executors.IImportJob> _importJobFake;
		private Mock<ISyncImportBulkArtifactJob> _syncImportBulkArtifactJobFake;
		private Mock<IItemStatusMonitor> _itemStatusMonitorFake;
		private Mock<IDocumentSynchronizationConfiguration> _configFake;

		private DocumentSynchronizationExecutor _sut;

		private const long _METADATA_SIZE = 2L;
		private const long _FILES_SIZE = 10L;
		private const long _JOB_SIZE = 12L;

		private const string _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME = "FolderPath_76B270CB-7CA9-4121-B9A1-BC0D655E5B2D";
		private const string _NATIVE_FILE_FILENAME_DISPLAY_NAME = "NativeFileFilename";
		private const string _NATIVE_FILE_SIZE_DISPLAY_NAME = "NativeFileSize";
		private const string _NATIVE_FILE_LOCATION_DISPLAY_NAME = "NativeFileLocation";
		private const string _SUPPORTED_BY_VIEWER_DISPLAY_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME = "RelativityNativeType";
		
		private const int _SOURCE_WORKSPACE_ID = 68;
		private const int _USER_ID = 70;

		private readonly List<FieldInfoDto> _specialFields = new List<FieldInfoDto>
		{
			FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME),
			FieldInfoDto.NativeFileSizeField(),
			FieldInfoDto.NativeFileLocationField(),
			FieldInfoDto.NativeFileFilenameField(),
			FieldInfoDto.RelativityNativeTypeField(),
			FieldInfoDto.SupportedByViewerField()
		};

		private Mock<IUserContextConfiguration> _userContextConfigurationStub;
		private BatchStub[] _batchesStubs;
		
		private const int _DATA_SOURCE_ID = 55;
		private const string _CORRELATION_ID = "CORRELATION_ID";

		public static (object[] BatchResults, object ExpectedResult)[] AggregationTestCaseSource { get; } =
		{
			(new object[]{ ExecutionStatus.Completed,ExecutionStatus.Completed, ExecutionStatus.Completed},
				ExecutionStatus.Completed),

			(new object[]{ ExecutionStatus.Completed, ExecutionStatus.Failed},
				ExecutionStatus.Failed),

			(new object[]{ ExecutionStatus.Completed, ExecutionStatus.CompletedWithErrors},
				ExecutionStatus.CompletedWithErrors),

			(new object[]{ ExecutionStatus.Completed, ExecutionStatus.CompletedWithErrors, ExecutionStatus.Canceled},
				ExecutionStatus.Canceled)
		};

		[SetUp]
		public void SetUp()
		{
			_importJobFactoryFake = new Mock<IImportJobFactory>();
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();
			_fieldManagerFake = new Mock<IFieldManager>();
			_fakeFieldMappings = new Mock<IFieldMappings>();
			_configFake = new Mock<IDocumentSynchronizationConfiguration>();
			_jobProgressHandlerFactoryStub = new Mock<IJobProgressHandlerFactory>();
			_jobCleanupConfigurationMock = new Mock<IJobCleanupConfiguration>();
			_automatedWorkflowTriggerConfigurationFake = new Mock<IAutomatedWorkflowTriggerConfiguration>();
			_jobProgressUpdaterFactoryStub = new Mock<IJobProgressUpdaterFactory>();
			_taggingProviderFake = new Mock<ITaggingProvider>();
			_taggingProviderFake
				.Setup(x => x.TagObjectsAsync(It.IsAny<Sync.Executors.IImportJob>(), It.IsAny<ISynchronizationConfiguration>(), It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(TaggingExecutionResult.Success);
			_stopwatchFactoryFake = new Mock<Func<IStopwatch>>();
			_stopwatchFake = new Mock<IStopwatch>();
			_stopwatchFactoryFake.Setup(x => x()).Returns(_stopwatchFake.Object);
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_jobStatisticsContainerFake.Setup(x => x.CalculateAverageLongTextStreamSizeAndTime(It.IsAny<Func<long, bool>>()))
				.Returns(new Tuple<double, double>(0, 0));
			_jobStatisticsContainerFake.SetupGet(x => x.LongTextStatistics).Returns(new List<LongTextStreamStatistics>());

			_jobProgressHandlerFake = new Mock<IJobProgressHandler>();
			_jobProgressUpdaterFake = new Mock<IJobProgressUpdater>();

			_jobProgressHandlerFactoryStub.Setup(x => x.CreateJobProgressHandler(It.IsAny<IEnumerable<IBatch>>(), It.IsAny<IScheduler>()))
				.Returns(_jobProgressHandlerFake.Object);

			_jobProgressUpdaterFactoryStub.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdaterFake.Object);

			_fakeFieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>
			{
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						IsIdentifier = true,
					}
				}
			});

			_itemStatusMonitorFake = new Mock<IItemStatusMonitor>();

			_syncImportBulkArtifactJobFake = new Mock<ISyncImportBulkArtifactJob>();
			_syncImportBulkArtifactJobFake.SetupGet(x => x.ItemStatusMonitor).Returns(_itemStatusMonitorFake.Object);

			_importJobFake = new Mock<Sync.Executors.IImportJob>();
			_importJobFake.SetupGet(x => x.SyncImportBulkArtifactJob).Returns(_syncImportBulkArtifactJobFake.Object);
			_importJobFactoryFake.Setup(x => x.CreateNativeImportJobAsync(It.IsAny<IDocumentSynchronizationConfiguration>(), It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).ReturnsAsync(_importJobFake.Object);
			
			_fieldManagerFake.Setup(x => x.GetNativeSpecialFields()).Returns(_specialFields);
			_userContextConfigurationStub = new Mock<IUserContextConfiguration>();

			_batchRepositoryMock.Setup(x => x.GetAllSuccessfullyExecutedBatchesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
				.ReturnsAsync(Enumerable.Empty<IBatch>());

			_sut = new DocumentSynchronizationExecutor(_importJobFactoryFake.Object, _batchRepositoryMock.Object,
				_jobProgressHandlerFactoryStub.Object,
				_fieldManagerFake.Object, _fakeFieldMappings.Object, _jobStatisticsContainerFake.Object,
				_jobCleanupConfigurationMock.Object, _automatedWorkflowTriggerConfigurationFake.Object,
				_stopwatchFactoryFake.Object, _syncMetricsMock.Object, _taggingProviderFake.Object, new EmptyLogger(), _userContextConfigurationStub.Object);
		}




		[Test]
		public async Task Execute_ShouldSendBatchMetrics()
		{
			// Arrange 
			const int totalRecordsTransferred = 111;
			const int totalRecordsRequested = 222;
			const int totalRecordsFailed = 333;
			const int totalRecordsTagged = 444;
			const int batchTime = 555;
			const int iapiTime = 666;

			Mock<IStopwatch> batchTimer = CreateFakeStopwatch(batchTime);
			Mock<IStopwatch> iapiTimer = CreateFakeStopwatch(iapiTime);
			_stopwatchFactoryFake.SetupSequence(x => x())
				.Returns(batchTimer.Object)
				.Returns(iapiTimer.Object);

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsProcessedCount(It.IsAny<int>())).Returns(totalRecordsTransferred);
			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsFailedCount(It.IsAny<int>())).Returns(totalRecordsFailed);

			SetupImportJob();

			IEnumerable<int> batches = new[] { 1 };
			_batchRepositoryMock.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(batches);
			BatchStub batchStub = new BatchStub
			{
				ArtifactId = 1,
				TotalDocumentsCount = totalRecordsRequested,
				StartingIndex = 0
			};
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchStub);


			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult(totalRecordsTagged);
			SetUpDocumentsTagRepository(executionResult);

			_jobStatisticsContainerFake
				.Setup(x => x.CalculateAverageLongTextStreamSizeAndTime(It.IsAny<Func<long, bool>>()))
				.Returns(new Tuple<double, double>(1, 2));
			_jobStatisticsContainerFake
				.SetupGet(x => x.LongTextStatistics)
				.Returns(Enumerable.Range(1, 20).Select(x => new LongTextStreamStatistics()
				{
					TotalBytesRead = x * 1024 * 1024,
					TotalReadTime = TimeSpan.FromSeconds(x)
				}).ToList());

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<DocumentBatchEndMetric>(m =>
				m.AvgSizeLessThan1MB == 1 &&
				m.AvgTimeLessThan1MB == 2 &&
				m.AvgSizeLessBetween1and10MB == 1 &&
				m.AvgTimeLessBetween1and10MB == 2 &&
				m.AvgSizeLessBetween10and20MB == 1 &&
				m.AvgTimeLessBetween10and20MB == 2 &&
				m.AvgSizeOver20MB == 1 &&
				m.AvgTimeOver20MB == 2 &&
				m.TotalRecordsTransferred == totalRecordsTransferred &&
				m.TotalRecordsRequested == totalRecordsRequested &&
				m.TotalRecordsFailed == totalRecordsFailed &&
				m.TotalRecordsTagged == totalRecordsTagged &&
				m.BytesMetadataTransferred == _METADATA_SIZE &&
				m.BytesNativesTransferred == _FILES_SIZE &&
				m.BytesTransferred == _FILES_SIZE + _METADATA_SIZE &&
				m.BatchTotalTime == batchTime &&
				m.BatchImportAPITime == iapiTime &&
				m.TopLongTexts.Count == 10)), Times.Once);
		}
		
		[Test]
		public async Task Execute_ShouldSendPerformanceMetrics()
		{
			// Arrange 
			const int totalRecordsTransferred = 111;
			const int totalRecordsRequested = 222;
			const int totalRecordsFailed = 333;
			const int totalRecordsTagged = 444;
			const int batchTime = 555;
			const int iapiTime = 2666;

			Mock<IStopwatch> batchTimer = CreateFakeStopwatch(batchTime);
			Mock<IStopwatch> iapiTimer = CreateFakeStopwatch(iapiTime);
			_stopwatchFactoryFake.SetupSequence(x => x())
				.Returns(batchTimer.Object)
				.Returns(iapiTimer.Object);

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsProcessedCount(It.IsAny<int>())).Returns(totalRecordsTransferred);
			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsFailedCount(It.IsAny<int>())).Returns(totalRecordsFailed);

			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(importJob);

			IEnumerable<int> batches = new[] { 1 };
			_batchRepositoryMock.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(batches);
			
			BatchStub batchStub = new BatchStub
			{
				ArtifactId = 1,
				TotalDocumentsCount = totalRecordsRequested,
				StartingIndex = 0
			};
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchStub);

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult(totalRecordsTagged);
			SetUpDocumentsTagRepository(executionResult);

			_jobStatisticsContainerFake
				.Setup(x => x.CalculateAverageLongTextStreamSizeAndTime(It.IsAny<Func<long, bool>>()))
				.Returns(new Tuple<double, double>(1, 2));
			_jobStatisticsContainerFake
				.SetupGet(x => x.LongTextStatistics)
				.Returns(Enumerable.Range(1, 20).Select(x => new LongTextStreamStatistics()
				{
					TotalBytesRead = x * 1024 * 1024,
					TotalReadTime = TimeSpan.FromSeconds(x)
				}).ToList());
			
			_syncMetricsMock.Setup(x => x.Send(It.IsAny<IMetric>())).Callback((IMetric m) => m.CorrelationId = _CORRELATION_ID);

			_jobCleanupConfigurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);

			_userContextConfigurationStub.Setup(x => x.ExecutingUserId).Returns(_USER_ID);
			_configFake.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			double bytesInGigabyte = 1024.0 * 1024 * 1024;

			// Assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<BatchEndPerformanceMetric>(m =>
				m.WorkflowName == "Relativity.Sync"
				&& m.StageName == "Transfer"
				&& m.Elapsed == iapiTime / 1000
				&& m.APMCategory == "PerformanceBatchJob"
				&& m.CorrelationId == _CORRELATION_ID
				&& m.JobID == 1
				&& m.WorkspaceID == _SOURCE_WORKSPACE_ID
				&& m.JobStatus == ExecutionStatus.Completed
				&& m.RecordNumber == totalRecordsTransferred
				&& m.RecordType == BatchRecordType.Documents
				&& m.JobSizeGB == _JOB_SIZE / bytesInGigabyte
				&& m.JobSizeGB_Metadata == _METADATA_SIZE / bytesInGigabyte
				&& m.JobSizeGB_Files == _FILES_SIZE / bytesInGigabyte
				&& m.UserID == _USER_ID
				&& m.SavedSearchID == _DATA_SOURCE_ID
			)));
		}

		[TestCase(ImportNativeFileCopyMode.CopyFiles)]
		[TestCase(ImportNativeFileCopyMode.SetFileLinks)]
		public async Task Execute_ShouldSetImportApiSettings_WhenImportingNatives(ImportNativeFileCopyMode importNativeCopyMode)
		{
			// Arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_configFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(importNativeCopyMode);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();
			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettings_WhenDoNotImportNatives()
		{
			// Arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_configFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();
			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Never);
		}

		[Test]
		public async Task Execute_ShouldCancelTaggingResultTest()
		{
			string expectedMessage = "Executing synchronization was interrupted due to the job being canceled.";
			// Arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);

			SetupImportJob();

			_taggingProviderFake.Setup(x => x.TagObjectsAsync(It.IsAny<Sync.Executors.IImportJob>(),
				It.IsAny<ISynchronizationConfiguration>(), It.IsAny<CompositeCancellationToken>())).Throws<OperationCanceledException>();

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Message.Should()
				.Be(expectedMessage);
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldCatchExceptionTest()
		{
			// Arrange
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);

			SetupImportJob();

			TaggingExecutionResult executionResult = null;

			SetUpDocumentsTagRepository(executionResult);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Message.Should()
				.Be("Unexpected exception occurred while executing synchronization.");
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettingsExceptFolderInfo()
		{
			//Arrange
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			_configFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

			SetupImportJob();

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();

			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = It.IsAny<string>(), Times.Never);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public async Task Execute_ShouldSetOnlyFolderInfo()
		{
			//Arrange
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_configFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

			SetupImportJob();

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();

			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = It.IsAny<string>(), Times.Once);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Never);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Never);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenDestinationIdentityFieldNotExistsInFieldMappings()
		{
			// Arrange 
			SetupBatchRepository(1);
			_fakeFieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			// Act
			Func<Task> action = () => _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None);
			// Assert
			string errorMessage = "Cannot find destination identifier field in field mappings.";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(errorMessage);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenSpecialFieldIsNotFound()
		{
			// Arrange 
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			_configFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);
			List<FieldInfoDto> specialFields = _specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fieldManagerFake.Setup(x => x.GetNativeSpecialFields()).Returns(specialFields);
			Func<Task> action = () => _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None);

			// Act
			action();

			// Assert
			string expectedMessage = $"Cannot find special field name: {missingSpecialField}";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(expectedMessage);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public async Task Execute_ShouldRunImportApiJobForEachBatch(int numberOfBatches)
		{
			// Arrange
			SetupBatchRepository(numberOfBatches);

			SetupImportJob();

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();

			SetUpDocumentsTagRepository(executionResult);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_batchRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numberOfBatches));
			_importJobFake.Verify(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()), Times.Exactly(numberOfBatches));
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task Execute_ShouldIncrementTaggedDocuments_WhenBatchHasBeenPaused()
		{
			// Arrange
			const int batchId = 1;
			const int totalItemsCount = 10;
			const int alreadyTaggedItems = 5;

			const int taggedAfterResume = 3;

			BatchStub batch = new BatchStub
			{
				ArtifactId = batchId,
				TotalDocumentsCount = totalItemsCount,
				StartingIndex = 0
			};
			await batch.SetTaggedDocumentsCountAsync(alreadyTaggedItems).ConfigureAwait(false);

			SetupBatch(batch);

			SetupImportJob();

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult(taggedAfterResume);

			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			batch.TaggedDocumentsCount.Should().Be(alreadyTaggedItems + taggedAfterResume);
		}

		[Test]
		public async Task Execute_ShouldCancelImportJob()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None);
			tokenSource.Cancel();

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, compositeCancellationToken).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldDisposeImportJob()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			TaggingExecutionResult executionResult = ReturnTaggingCompletedResult();
			SetUpDocumentsTagRepository(executionResult);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_importJobFake.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		public static IEnumerable<ExecutionResult> BrakingExecutionResults => new[] { ExecutionResult.Failure(new SyncException()), ExecutionResult.Canceled() };

		[Test]
		[TestCaseSource(nameof(BrakingExecutionResults))]
		public async Task Execute_ShouldBreak_WhenPushingOrTaggingBrakes(ExecutionResult expectedExecutionResult)
		{
			// Arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(new ImportJobResult(expectedExecutionResult, 1, 0, 1));

			SetUpDocumentsTagRepository(CastToTaggingResult(expectedExecutionResult));

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().BeEquivalentTo(expectedExecutionResult.Status);
		}

		[Test]
		[TestCaseSource(nameof(BrakingExecutionResults))]
		public async Task Execute_ShouldRunTagging_WhenPushingBrakes(ExecutionResult expectedExecutionResult)
		{
			// Arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(new ImportJobResult(expectedExecutionResult, 1, 0, 1));

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().BeEquivalentTo(expectedExecutionResult.Status);

			_taggingProviderFake
				.Verify(x => x.TagObjectsAsync(_importJobFake.Object,
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<CompositeCancellationToken>()), Times.Once);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportSyncException()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).Throws<ImportFailedException>();

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			// Assert
			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<ImportFailedException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleAnyImportException()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).Throws<InvalidOperationException>();

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldReturnFailed()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			TaggingExecutionResult executionResult = ReturnTaggingFailedResult();
			SetUpDocumentsTagRepository(executionResult);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			// Assert
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			// Arrange 
			const int numberOfBatches = 2;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).Throws<InvalidOperationException>();

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenCompletedSuccessfully()
		{
			// Arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenFailed()
		{
			// Arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(CreateJobResult());

			TaggingExecutionResult executionResult = ReturnTaggingFailedResult();
			SetUpDocumentsTagRepository(executionResult);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		[Test, TestCaseSource(nameof(AggregationTestCaseSource))]
		public async Task Execute_ShouldCorrectlyAggregateBatchJobResults(
			(object[] batchJobResultsObject, object expectedResultStatus) testCase)
		{
			// Arrange
			Queue<ImportJobResult> batchJobResults = new Queue<ImportJobResult>(
				testCase.batchJobResultsObject.Select(
					x => GetJobResult((ExecutionStatus)x, exception: new Exception())));
			SetupBatchRepository(testCase.batchJobResultsObject.Length);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(() => batchJobResults.Dequeue());

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(testCase.expectedResultStatus);
		}

		[Test]
		public async Task Execute_ShouldRespectCancellationBetweenBatches()
		{
			// Arrange
			const int batchCount = 2;
			SetupBatchRepository(batchCount);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(cancellationTokenSource.Token, CancellationToken.None);

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(CreateJobResult(ExecutionResult.Success()))
				.Callback(cancellationTokenSource.Cancel);

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, compositeCancellationToken)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
			_importJobFake.Verify(x => x.RunAsync(compositeCancellationToken), Times.Once);
		}

		[Test]
		public async Task Execute_ShouldMarkBatchAsPaused_WhenOnDrainStopRequested()
		{
			// Arrange
 			const int batchCount = 3;
			SetupBatchRepository(batchCount);
			//SetUpDocumentsTagRepository(ReturnTaggingCompletedResultAsync());

			CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(CancellationToken.None, drainStopCancellationTokenSource.Token);

			_importJobFake.SetupSequence(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.Returns(Task.FromResult(CreateJobResult()))
				.Returns(Task.Run(() =>
				{
					drainStopCancellationTokenSource.Cancel();
					return Task.FromResult(CreatePausedResult());
				}));
				

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, compositeCancellationToken)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Paused);
			_importJobFake.Verify(x => x.RunAsync(compositeCancellationToken), Times.Exactly(2));
			_batchesStubs[0].Status.Should().Be(BatchStatus.Completed);
			_batchesStubs[1].Status.Should().Be(BatchStatus.Paused);
			_batchesStubs[2].Status.Should().Be(BatchStatus.New);
		}

		[TestCase(ExecutionStatus.Canceled, BatchStatus.Cancelled)]
		[TestCase(ExecutionStatus.Completed, BatchStatus.Completed)]
		[TestCase(ExecutionStatus.CompletedWithErrors, BatchStatus.CompletedWithErrors)]
		[TestCase(ExecutionStatus.Paused, BatchStatus.Paused)]
		public async Task Execute_ShouldSetBatchStatus(ExecutionStatus executionStatus, BatchStatus batchStatus)
		{
			// Arrange
			SetupBatchRepository(1);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync( new ImportJobResult(new ExecutionResult(executionStatus, "", null), 0 ,0, 0));
			
			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);
			
			// Assert
			result.Status.Should().Be(executionStatus);
			_batchesStubs.First().Status.Should().Be(batchStatus);
		}

		[Test]
		public async Task ExecuteAsync_ShouldUpdateFailedDocumentsCountInAdditiveManner()
		{
			// Arrange
			const int initialFailedDocumentsCount = 3;
			const int failedDocumentsCountInRun = 2;

			IBatch batch = new BatchStub
			{
				FailedDocumentsCount = initialFailedDocumentsCount
			};

			SetupBatch(batch);

			SetupImportJob();

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_itemStatusMonitorFake.SetupGet(x => x.FailedItemsCount).Returns(failedDocumentsCountInRun);
			
			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			const int expectedFailedDocumentsCount = initialFailedDocumentsCount + failedDocumentsCountInRun;
			
			batch.FailedDocumentsCount.Should().Be(expectedFailedDocumentsCount);
		}

		[Test]
		public async Task ExecuteAsync_ShouldUpdateTransferredDocumentsCountInAdditiveManner()
		{
			// Arrange
			const int initialTransferredDocumentsCount = 3;
			const int transferredDocumentsCountInRun = 2;

			IBatch batch = new BatchStub
			{
				TransferredDocumentsCount = initialTransferredDocumentsCount
			};

			SetupBatch(batch);

			SetupImportJob();

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_itemStatusMonitorFake.SetupGet(x => x.ProcessedItemsCount).Returns(transferredDocumentsCountInRun);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			const int expectedTransferredDocumentsCount = initialTransferredDocumentsCount + transferredDocumentsCountInRun;

			batch.TransferredDocumentsCount.Should().Be(expectedTransferredDocumentsCount);
		}

		[Test]
		public async Task ExecuteAsync_ShouldUpdateFailedItemsCountInAdditiveManner()
		{
			// Arrange
			const int initialFailedItemsCount = 3;
			const int failedItemsCountInRun = 2;

			IBatch batch = new BatchStub
			{
				FailedItemsCount = initialFailedItemsCount
			};

			SetupBatch(batch);

			SetupImportJob();

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsFailedCount(It.IsAny<int>()))
				.Returns(failedItemsCountInRun);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			const int expectedFailedItemsCount = initialFailedItemsCount + failedItemsCountInRun;

			batch.FailedItemsCount.Should().Be(expectedFailedItemsCount);
		}

		[Test]
		public async Task ExecuteAsync_ShouldUpdateTransferredItemsCountInAdditiveManner()
		{
			// Arrange
			const int initialTransferredItemsCount = 3;
			const int transferredItemsCountInRun = 2;

			IBatch batch = new BatchStub
			{
				TransferredItemsCount = initialTransferredItemsCount
			};

			SetupBatch(batch);

			SetupImportJob();

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsProcessedCount(It.IsAny<int>()))
				.Returns(transferredItemsCountInRun);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			const int expectedTransferredItemsCount = initialTransferredItemsCount + transferredItemsCountInRun;

			batch.TransferredItemsCount.Should().Be(expectedTransferredItemsCount);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnCompletedWithErrors_WhenBatchWithItemLevelErrorsCompletesWithoutAnyErrorsAfterResume()
		{
			// Arrange
			const int initialFailedDocumentsCount = 1;

			IBatch batch = new BatchStub
			{
				FailedDocumentsCount = initialFailedDocumentsCount
			};

			SetupBatch(batch);

			SetupImportJob();

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());

			_itemStatusMonitorFake.SetupGet(x => x.FailedItemsCount).Returns(0);

			// Act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			batch.Status.Should().Be(BatchStatus.CompletedWithErrors);
		}

		[TestCase(0, 0, 0, 0, 0, 0, 10, 2, 3024, 6144, 9168, 3, ExecutionStatus.Paused, BatchStatus.Paused, 5)]
		[TestCase(3, 1, 2, 2048, 4096, 6144, 10, 2, 3024, 6144, 9168, 3, ExecutionStatus.Paused, BatchStatus.Paused, 8)]
		[TestCase(0, 0, 0, 0, 0, 0, 10, 0, 10240, 20480, 30720, 10, ExecutionStatus.Completed, BatchStatus.Completed, 0)]
		[TestCase(0, 2, 3, 3024, 6144, 9168, 10, 0, 5120, 10240, 15360, 5, ExecutionStatus.CompletedWithErrors, BatchStatus.CompletedWithErrors, 0)]
		[TestCase(1500, 0, 0, 0, 0, 0, 500, 0, 41062078, 18705240, 59767318, 108, ExecutionStatus.Paused, BatchStatus.Paused, 1608)]
		[TestCase(1608, 0, 108, 41062078, 18705240, 59767318, 500, 0, 0, 0, 0, 0, ExecutionStatus.Paused, BatchStatus.Paused, 1608)]
		public async Task Execute_ShouldHandlePausedBatch(
			int initialStartingIndex, int initialFailedCount, int initialTransferredCount, int initialMetadataBytesTransferred, int initialFilesBytesTransferred, int initialTotalBytesTransferred, int totalCount,
			int failedCount, int metadataBytesTransferred, int filesBytesTransferred, int totalBytesTransferred, int transferredCount,
			ExecutionStatus expectedStatus, BatchStatus expectedBatchStatus, int expectedStartingIndex)
		{
			// Arrange
			IBatch batch = new BatchStub
			{
				StartingIndex = initialStartingIndex,
				FailedDocumentsCount = initialFailedCount,
				TransferredDocumentsCount = initialTransferredCount,
				TotalDocumentsCount = totalCount,
				MetadataBytesTransferred = initialMetadataBytesTransferred,
				FilesBytesTransferred = initialFilesBytesTransferred,
				TotalBytesTransferred = initialTotalBytesTransferred
			};

			SetupBatch(batch);

			SetupImportJob(ExecutionResult.Paused(), metadataBytesTransferred, filesBytesTransferred, totalBytesTransferred);

			SetUpDocumentsTagRepository(ReturnTaggingCompletedResult());
			
			_itemStatusMonitorFake.SetupGet(x => x.FailedItemsCount).Returns(failedCount);
			_itemStatusMonitorFake.SetupGet(x => x.ProcessedItemsCount).Returns(transferredCount);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(expectedStatus);

			batch.Status.Should().Be(expectedBatchStatus);
			batch.StartingIndex.Should().Be(expectedStartingIndex);
			batch.TransferredDocumentsCount.Should().Be(initialTransferredCount + transferredCount);
			batch.MetadataBytesTransferred.Should().Be(initialMetadataBytesTransferred + metadataBytesTransferred);
			batch.FilesBytesTransferred.Should().Be(initialFilesBytesTransferred + filesBytesTransferred);
			batch.TotalBytesTransferred.Should().Be(initialTotalBytesTransferred + totalBytesTransferred);
		}

		private void SetupBatch(IBatch batch)
		{
			_batchRepositoryMock.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
				.ReturnsAsync(new int[] { batch.ArtifactId });
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(batch);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			const int itemsPerBatch = 10;
			_batchesStubs = Enumerable.Range(1, numberOfBatches).Select(x => new BatchStub
			{
				ArtifactId = x,
				TotalDocumentsCount = itemsPerBatch,
				StartingIndex = x * itemsPerBatch
			}).ToArray();
			

			_batchRepositoryMock.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(_batchesStubs.Select(x => x.ArtifactId));
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((int workspaceId, int batchId) => _batchesStubs.First(x => x.ArtifactId == batchId));
		}

		private void SetupImportJob(ExecutionResult result = null, long metadataBytesTransferred = _METADATA_SIZE, long filesBytesTransferred = _FILES_SIZE, long totalBytesTransferred = _JOB_SIZE)
		{
			ExecutionResult jobResult = result ?? ExecutionResult.Success();

			ImportJobResult importJob = new ImportJobResult(jobResult, metadataBytesTransferred, filesBytesTransferred, totalBytesTransferred);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CompositeCancellationToken>())).ReturnsAsync(importJob);
		}

		private void SetUpDocumentsTagRepository(TaggingExecutionResult executionResult)
		{
			_taggingProviderFake
				.Setup(x => x.TagObjectsAsync(It.IsAny<Sync.Executors.IImportJob>(), It.IsAny<ISynchronizationConfiguration>(), It.IsAny<CompositeCancellationToken>()))
				.ReturnsAsync(executionResult);
		}

		private static TaggingExecutionResult ReturnTaggingCompletedResult(int taggedCount = 0)
		{
			return new TaggingExecutionResult(ExecutionStatus.Completed, "Completed", new Exception())
			{
				TaggedDocumentsCount = taggedCount
			};
		}

		private static TaggingExecutionResult ReturnTaggingFailedResult()
		{
			return new TaggingExecutionResult(ExecutionStatus.Failed, "Failed", new Exception());
		}

		private static TaggingExecutionResult CastToTaggingResult(ExecutionResult result)
			=> new TaggingExecutionResult(result.Status, result.Message, result.Exception);

		private static ImportJobResult CreateJobResult(ExecutionResult result = null, int metadataBytesTransferred = 1, int filesBytesTransferred = 0, int totalBytesTransferred = 1)
		{
			ExecutionResult jobResult = result ?? ExecutionResult.Success();

			return new ImportJobResult(jobResult, metadataBytesTransferred, filesBytesTransferred, totalBytesTransferred);
		}
		
		private static ImportJobResult CreatePausedResult()
		{
			return new ImportJobResult(ExecutionResult.Paused(), 1, 0, 1);
		}

		private static ImportJobResult GetJobResult(ExecutionStatus status, string message = null, Exception exception = null)
		{
			return new ImportJobResult(new ExecutionResult(status, message ?? exception?.Message, exception), 1, 0, 1);
		}

		private static Mock<IStopwatch> CreateFakeStopwatch(int elapsedMs)
		{
			Mock<IStopwatch> batchTimer = new Mock<IStopwatch>();
			batchTimer.SetupGet(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(elapsedMs));
			return batchTimer;
		}
	}
}