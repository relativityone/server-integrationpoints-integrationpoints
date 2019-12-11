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
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal class SynchronizationExecutorTests
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

		private Mock<Sync.Executors.IImportJob> _importJobFake;
		private Mock<ISynchronizationConfiguration> _configFake;

		private SynchronizationExecutor _sut;

		private const string _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME = "76B270CB-7CA9-4121-B9A1-BC0D655E5B2D";
		private const string _NATIVE_FILE_FILENAME_DISPLAY_NAME = "NativeFileFilename";
		private const string _NATIVE_FILE_SIZE_DISPLAY_NAME = "NativeFileSize";
		private const string _NATIVE_FILE_LOCATION_DISPLAY_NAME = "NativeFileLocation";
		private const string _SUPPORTED_BY_VIEWER_DISPLAY_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME = "RelativityNativeType";

		private readonly List<FieldInfoDto> _specialFields = new List<FieldInfoDto>
		{
			FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME),
			FieldInfoDto.NativeFileSizeField(),
			FieldInfoDto.NativeFileLocationField(),
			FieldInfoDto.NativeFileFilenameField(),
			FieldInfoDto.RelativityNativeTypeField(),
			FieldInfoDto.SupportedByViewerField()
		};

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
			_configFake = new Mock<ISynchronizationConfiguration>();
			_jobProgressHandlerFactoryStub = new Mock<IJobProgressHandlerFactory>();
			_jobCleanupConfigurationMock = new Mock<IJobCleanupConfiguration>();
			_jobProgressUpdaterFactoryStub = new Mock<IJobProgressUpdaterFactory>();

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
			_importJobFactoryFake.Setup(x => x.CreateImportJobAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).ReturnsAsync(_importJobFake.Object);

			_fieldManagerFake.Setup(x => x.GetSpecialFields()).Returns(_specialFields);

			_sut = new SynchronizationExecutor(_importJobFactoryFake.Object, _batchRepositoryMock.Object,
				_jobProgressHandlerFactoryStub.Object,
				_documentTagRepositoryFake.Object, _fieldManagerFake.Object, _fakeFieldMappings.Object, _jobStatisticsContainerFake.Object,
				_jobCleanupConfigurationMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettings()
		{
			// arrange 
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(tokenSource.Token);
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should()
				.Be("Executing synchronization was interrupted due to the job being canceled.");
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldCatchExceptionTest()
		{
			// arrange 
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = null;

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should()
				.Be("Unexpected exception occurred while executing synchronization.");
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettingsExceptFolderInfo()
		{
			//Arrange
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_configFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			Func<Task> action = async () => await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);
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
			_fieldManagerFake.Setup(x => x.GetSpecialFields()).Returns(specialFields);
			Func<Task> action = async () => await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			const long jobSize = 12L;
			SetupBatchRepository(numberOfBatches);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			tokenSource.Cancel();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, tokenSource.Token).ConfigureAwait(false);

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

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_importJobFake.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		public static IEnumerable<ExecutionResult> BrakingExecutionResults => new[] { ExecutionResult.Failure(new SyncException()), ExecutionResult.Canceled() };
		public static IEnumerable<Action<SynchronizationExecutorTests, ExecutionResult>> BrakingActionsSetups => new Action<SynchronizationExecutorTests, ExecutionResult>[]
		{
			(ctx, result) => ctx._documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(result),
			(ctx, result) => ctx._documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(result),
			(ctx, result) => ctx._importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(result, 1))
		};

		[Test]
		public async Task Execute_ShouldBreak_WhenPushingOrTaggingBrakes(
			[ValueSource(nameof(BrakingExecutionResults))] ExecutionResult expectedExecutionResult,
			[ValueSource(nameof(BrakingActionsSetups))] Action<SynchronizationExecutorTests, ExecutionResult> brakingActionSetup)
		{
			// arrange
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(ExecutionResult.Success);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(ExecutionResult.Success);

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(ExecutionResult.Success(), 1));

			brakingActionSetup(this, expectedExecutionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
					.ReturnsAsync(ExecutionResult.Success);

			_documentTagRepositoryFake
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
					.ReturnsAsync(ExecutionResult.Success);

			_importJobFake
				.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ImportJobResult(expectedExecutionResult, 1));

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);
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
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);
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
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_configFake.Object, CancellationToken.None).ConfigureAwait(false);

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
				.ExecuteAsync(_configFake.Object, CancellationToken.None)
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

			_importJobFake.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateSuccessfulResult)
				.Callback(cancellationTokenSource.Cancel);

			// act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configFake.Object, cancellationTokenSource.Token)
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

		private void SetUpDocumentsTagRepository(Task<ExecutionResult> executionResult)
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

		private static async Task<ExecutionResult> ReturnTaggingCompletedResultAsync()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new ExecutionResult(ExecutionStatus.Completed, "Completed", new Exception());
		}
		private static async Task<ExecutionResult> ReturnTaggingCompletedResultAsync(CancellationToken cancellationToken)
		{
			await Task.CompletedTask.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			return new ExecutionResult(ExecutionStatus.Completed, "Completed", new Exception());
		}

		private static async Task<ExecutionResult> ReturnTaggingFailedResultAsync()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new ExecutionResult(ExecutionStatus.Failed, "Failed", new Exception());
		}
		private static ImportJobResult CreateSuccessfulResult()
		{
			return new ImportJobResult(ExecutionResult.Success(), 1);
		}

		private static ImportJobResult GetJobResult(ExecutionStatus status, string message = null, Exception exception = null, int jobSize = 1)
		{
			return new ImportJobResult(new ExecutionResult(status, message ?? exception?.Message, exception), jobSize);
		}
	}
}