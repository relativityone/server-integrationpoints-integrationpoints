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
		private Mock<IDocumentTagRepository> _documentTagRepositoryFake;
		private Mock<IImportJobFactory> _importJobFactoryFake;
		private Mock<IJobCleanupConfiguration> _jobCleanupConfigurationMock;
		private Mock<IJobProgressHandlerFactory> _jobProgressHandlerFactoryStub;
		private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactoryStub;
		private Mock<IJobProgressHandler> _jobProgressHandlerFake;
		private Mock<IJobProgressUpdater> _jobProgressUpdaterFake;
		private Mock<IAutomatedWorkflowTriggerConfiguration> _automatedWorkflowTriggerConfigurationFake;
		private Mock<Func<IStopwatch>> _stopwatchFactoryFake;
		private Mock<IStopwatch> _stopwatchFake;
		private Mock<ISyncMetrics> _syncMetricsMock;

		private Mock<Sync.Executors.IImportJob> _importJobFake;
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
		private const int _DATA_SOURCE_ID = 55;
		private const string _WORKFLOW_ID = "WORKFLOW_ID";

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
			_documentTagRepositoryFake = new Mock<IDocumentTagRepository>();
			_importJobFactoryFake = new Mock<IImportJobFactory>();
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();
			_fieldManagerFake = new Mock<IFieldManager>();
			_fakeFieldMappings = new Mock<IFieldMappings>();
			_documentTagRepositoryFake = new Mock<IDocumentTagRepository>();
			_configFake = new Mock<IDocumentSynchronizationConfiguration>();
			_jobProgressHandlerFactoryStub = new Mock<IJobProgressHandlerFactory>();
			_jobCleanupConfigurationMock = new Mock<IJobCleanupConfiguration>();
			_automatedWorkflowTriggerConfigurationFake = new Mock<IAutomatedWorkflowTriggerConfiguration>();
			_jobProgressUpdaterFactoryStub = new Mock<IJobProgressUpdaterFactory>();
			_stopwatchFactoryFake = new Mock<Func<IStopwatch>>();
			_stopwatchFake = new Mock<IStopwatch>();
			_stopwatchFactoryFake.Setup(x => x.Invoke()).Returns(_stopwatchFake.Object);
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_jobStatisticsContainerFake.Setup(x => x.CalculateAverageLongTextStreamSizeAndTime(It.IsAny<Func<long, bool>>()))
				.Returns(new Tuple<double, double>(0, 0));
			_jobStatisticsContainerFake.SetupGet(x => x.LongTextStatistics).Returns(new List<LongTextStreamStatistics>());

			_jobProgressHandlerFake = new Mock<IJobProgressHandler>();
			_jobProgressUpdaterFake = new Mock<IJobProgressUpdater>();

			_jobProgressHandlerFactoryStub.Setup(x => x.CreateJobProgressHandler(It.IsAny<IScheduler>()))
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

			_importJobFake = new Mock<Sync.Executors.IImportJob>();
			_importJobFactoryFake.Setup(x => x.CreateNativeImportJobAsync(It.IsAny<IDocumentSynchronizationConfiguration>(), It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).ReturnsAsync(_importJobFake.Object);

			_fieldManagerFake.Setup(x => x.GetNativeSpecialFields()).Returns(_specialFields);
			_userContextConfigurationStub = new Mock<IUserContextConfiguration>();

			

			_sut = new DocumentSynchronizationExecutor(_importJobFactoryFake.Object, _batchRepositoryMock.Object,
				_jobProgressHandlerFactoryStub.Object,
				_documentTagRepositoryFake.Object, _fieldManagerFake.Object, _fakeFieldMappings.Object, _jobStatisticsContainerFake.Object,
				_jobCleanupConfigurationMock.Object, _automatedWorkflowTriggerConfigurationFake.Object,
				_stopwatchFactoryFake.Object, _syncMetricsMock.Object,new EmptyLogger(), _userContextConfigurationStub.Object);
		}

		[Test]
		public async Task Execute_ShouldSendBatchMetrics()
		{
			// arrange 
			const int totalRecordsTransferred = 111;
			const int totalRecordsRequested = 222;
			const int totalRecordsFailed = 333;
			const int totalRecordsTagged = 444;
			const int batchTime = 555;
			const int iapiTime = 666;

			Mock<IStopwatch> batchTimer = CreateFakeStopwatch(batchTime);
			Mock<IStopwatch> iapiTimer = CreateFakeStopwatch(iapiTime);
			_stopwatchFactoryFake.SetupSequence(x => x.Invoke())
				.Returns(batchTimer.Object)
				.Returns(iapiTimer.Object);

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsProcessedCount(It.IsAny<int>())).Returns(totalRecordsTransferred);
			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsFailedCount(It.IsAny<int>())).Returns(totalRecordsFailed);

			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			IEnumerable<int> batches = new[] { 1 };
			_batchRepositoryMock.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			Mock<IBatch> batchFake = new Mock<IBatch>();
			batchFake.SetupGet(x => x.TotalItemsCount).Returns(totalRecordsRequested);
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchFake.Object);

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(totalRecordsTagged);
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

			// act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
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
			// arrange 
			const int totalRecordsTransferred = 111;
			const int totalRecordsRequested = 222;
			const int totalRecordsFailed = 333;
			const int totalRecordsTagged = 444;
			const int batchTime = 555;
			const int iapiTime = 2666;

			Mock<IStopwatch> batchTimer = CreateFakeStopwatch(batchTime);
			Mock<IStopwatch> iapiTimer = CreateFakeStopwatch(iapiTime);
			_stopwatchFactoryFake.SetupSequence(x => x.Invoke())
				.Returns(batchTimer.Object)
				.Returns(iapiTimer.Object);

			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsProcessedCount(It.IsAny<int>())).Returns(totalRecordsTransferred);
			_jobProgressHandlerFake.Setup(x => x.GetBatchItemsFailedCount(It.IsAny<int>())).Returns(totalRecordsFailed);

			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			IEnumerable<int> batches = new[] { 1 };
			_batchRepositoryMock.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			Mock<IBatch> batchFake = new Mock<IBatch>();
			batchFake.SetupGet(x => x.TotalItemsCount).Returns(totalRecordsRequested);



			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batchFake.Object);

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(totalRecordsTagged);
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
			
			_syncMetricsMock.Setup(x => x.Send(It.IsAny<IMetric>())).Callback((IMetric m) => m.WorkflowId = _WORKFLOW_ID);

			_jobCleanupConfigurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);

			_userContextConfigurationStub.Setup(x => x.ExecutingUserId).Returns(_USER_ID);
			_configFake.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			double bytesInGigabyte = 1024.0 * 1024 * 1024;

			// assert
			_syncMetricsMock.Verify(x => x.Send(It.Is<BatchEndPerformanceMetric>(m =>
				m.WorkflowName == "Relativity.Sync"
				&& m.StageName == "Transfer"
				&& m.Elapsed == iapiTime / 1000
				&& m.APMCategory == "PerformanceBatchJob"
				&& m.CorrelationID == _WORKFLOW_ID
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

		[Test]
		public async Task Execute_ShouldSetImportApiSettings()
		{
			// arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public async Task Execute_ShouldCancelTaggingResultTest()
		{
			// arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(tokenSource.Token);
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should()
				.Be("Executing synchronization was interrupted due to the job being canceled.");
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldCatchExceptionTest()
		{
			// arrange
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<TaggingExecutionResult> executionResult = null;

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
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
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_configFake.VerifySet(x => x.FolderPathSourceFieldName = It.IsAny<string>(), Times.Never);
			_configFake.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_configFake.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenDestinationIdentityFieldNotExistsInFieldMappings()
		{
			// arrange 
			SetupBatchRepository(1);
			_fakeFieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			// act
			Func<Task> action = () => _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None);
			// assert
			string errorMessage = "Cannot find destination identifier field in field mappings.";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(errorMessage);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenSpecialFieldIsNotFound()
		{
			// arrange 
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			List<FieldInfoDto> specialFields = _specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fieldManagerFake.Setup(x => x.GetNativeSpecialFields()).Returns(specialFields);
			Func<Task> action = () => _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None);

			// act
			action();

			// assert
			string expectedMessage = $"Cannot find special field name: {missingSpecialField}";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(expectedMessage);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public async Task Execute_ShouldRunImportApiJobForEachBatch(int numberOfBatches)
		{
			// arrange
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), _METADATA_SIZE, _FILES_SIZE, _JOB_SIZE);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_batchRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numberOfBatches));
			_importJobFake.Verify(x => x.RunAsync(CancellationToken.None), Times.Exactly(numberOfBatches));
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task Execute_ShouldCancelImportJob()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None);
			tokenSource.Cancel();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, compositeCancellationToken).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldDisposeImportJob()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<TaggingExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_importJobFake.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		public static IEnumerable<ExecutionResult> BrakingExecutionResults => new[] { ExecutionResult.Failure(new SyncException()), ExecutionResult.Canceled() };

		public static IEnumerable<Action<DocumentSynchronizationExecutorTests, ExecutionResult>> BrakingActionsSetups => new Action<DocumentSynchronizationExecutorTests, ExecutionResult>[]
		{
			(ctx, result) => ctx._documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(CastToTaggingResult(result)),
			(ctx, result) => ctx._documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(CastToTaggingResult(result)),
			(ctx, result) => ctx._importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(result, 1, 0, 1))
		};

		[Test]
		public async Task Execute_ShouldBreak_WhenPushingOrTaggingBrakes(
			[ValueSource(nameof(BrakingExecutionResults))] ExecutionResult expectedExecutionResult,
			[ValueSource(nameof(BrakingActionsSetups))] Action<DocumentSynchronizationExecutorTests, ExecutionResult> brakingActionSetup)
		{
			// arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(TaggingExecutionResult.Success());

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(TaggingExecutionResult.Success());

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(ExecutionResult.Success(), 1, 0, 1));

			brakingActionSetup(this, expectedExecutionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().BeEquivalentTo(expectedExecutionResult.Status);
		}

		[Test]
		[TestCaseSource(nameof(BrakingExecutionResults))]
		public async Task Execute_ShouldRunTagging_WhenPushingBrakes(ExecutionResult expectedExecutionResult)
		{
			// arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(TaggingExecutionResult.Success());

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(TaggingExecutionResult.Success());

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(expectedExecutionResult, 1, 0, 1));

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().BeEquivalentTo(expectedExecutionResult.Status);

			_documentTagRepositoryFake
				.Verify(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>()), Times.Once);

			_documentTagRepositoryFake
				.Verify(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportSyncException()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<ImportFailedException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			// assert
			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<ImportFailedException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleAnyImportException()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldReturnFailed()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<TaggingExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			// assert
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			// arrange 
			const int numberOfBatches = 2;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenCompletedSuccessfully()
		{
			// arrange 
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenFailed()
		{
			// arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<TaggingExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		[Test, TestCaseSource(nameof(AggregationTestCaseSource))]
		public async Task Execute_ShouldCorrectlyAggregateBatchJobResults(
			(object[] batchJobResultsObject, object expectedResultStatus) testCase)
		{
			// arrange
			Queue<ImportJobResult> batchJobResults = new Queue<ImportJobResult>(
				testCase.batchJobResultsObject.Select(
					x => GetJobResult((ExecutionStatus)x, exception: new Exception())));
			SetupBatchRepository(testCase.batchJobResultsObject.Length);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResultAsync());

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => batchJobResults.Dequeue());

			// act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// assert
			result.Status.Should().Be(testCase.expectedResultStatus);
		}

		[Test]
		public async Task Execute_ShouldRespectCancellationBetweenBatches()
		{
			// arrange
			const int batchCount = 2;
			SetupBatchRepository(batchCount);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResultAsync());

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(cancellationTokenSource.Token, CancellationToken.None);

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateSuccessfulResult)
				.Callback(cancellationTokenSource.Cancel);

			// act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, compositeCancellationToken)
				.ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
			_importJobFake.Verify(x => x.RunAsync(cancellationTokenSource.Token), Times.Once);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);

			_batchRepositoryMock.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);
		}

		private void SetUpDocumentsTagRepository(Task<TaggingExecutionResult> executionResult)
		{
			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>())).Returns(executionResult);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>())).Returns(executionResult);
		}

		private static Task<TaggingExecutionResult> ReturnTaggingCompletedResultAsync(int taggedCount = 0)
		{
			return Task.FromResult(new TaggingExecutionResult(ExecutionStatus.Completed, "Completed", new Exception())
			{
				TaggedDocumentsCount = taggedCount
			});
		}

		private static async Task<TaggingExecutionResult> ReturnTaggingCompletedResultAsync(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
			cancellationToken.ThrowIfCancellationRequested();
			return new TaggingExecutionResult(ExecutionStatus.Completed, "Completed", new Exception());
		}

		private static Task<TaggingExecutionResult> ReturnTaggingFailedResultAsync()
		{
			return Task.FromResult(new TaggingExecutionResult(ExecutionStatus.Failed, "Failed", new Exception()));
		}

		private static TaggingExecutionResult CastToTaggingResult(ExecutionResult result)
			=> new TaggingExecutionResult(result.Status, result.Message, result.Exception);

		private static ImportJobResult CreateSuccessfulResult()
		{
			return new ImportJobResult(ExecutionResult.Success(), 1, 0, 1);
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