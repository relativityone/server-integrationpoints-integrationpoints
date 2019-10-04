﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Scripts.Models;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class SynchronizationExecutorTests
	{
		private Mock<IBatchRepository> _batchRepository;
		private Mock<IFieldManager> _fieldManager;
		private Mock<IFieldMappings> _fieldMappings;
		private Mock<IJobStatisticsContainer> _jobStatisticsContainer;
		private Mock<IDocumentTagRepository> _documentTagRepository;
		private Mock<IImportJobFactory> _importJobFactory;

		private Mock<Sync.Executors.IImportJob> _importJob;
		private Mock<ISynchronizationConfiguration> _config;

		private SynchronizationExecutor _synchronizationExecutor;

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
			_documentTagRepository = new Mock<IDocumentTagRepository>();
			_importJobFactory = new Mock<IImportJobFactory>();
			_batchRepository = new Mock<IBatchRepository>();
			_jobStatisticsContainer = new Mock<IJobStatisticsContainer>();
			_fieldManager = new Mock<IFieldManager>();
			_fieldMappings = new Mock<IFieldMappings>();
			_documentTagRepository = new Mock<IDocumentTagRepository>();
			_config = new Mock<ISynchronizationConfiguration>();

			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>
			{
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						IsIdentifier = true,
					}
				}
			});

			_importJob = new Mock<Sync.Executors.IImportJob>();
			_importJobFactory.Setup(x => x.CreateImportJobAsync(It.IsAny<ISynchronizationConfiguration>(),
				It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).ReturnsAsync(_importJob.Object);

			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(_specialFields);

			_synchronizationExecutor = new SynchronizationExecutor(_importJobFactory.Object, _batchRepository.Object,
				_documentTagRepository.Object, _fieldManager.Object, _fieldMappings.Object,
				_jobStatisticsContainer.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldSetImportApiSettings()
		{
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior)
				.Returns(DestinationFolderStructureBehavior.ReadFromField);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_config.VerifySet(x => x.FolderPathSourceFieldName = _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}


		[Test]
		public async Task ItShouldCancelTaggingResultTest()
		{
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior)
				.Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(tokenSource.Token);
			SetUpDocumentsTagRepository(executionResult);

			//Act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			//Assert
			result.Message.Should()
				.Be("Tagging synchronized documents in workspace was interrupted due to the job being canceled.");
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task ItShouldCatchExceptionTest()
		{
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior)
				.Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = null;

			SetUpDocumentsTagRepository(executionResult);

			//Act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			//Assert
			result.Message.Should()
				.Be("Unexpected exception occurred while tagging synchronized documents in workspace.");
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldSetImportApiSettingsExceptFolderInfo()
		{
			//Arrange
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior)
				.Returns(DestinationFolderStructureBehavior.None);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_config.VerifySet(x => x.FolderPathSourceFieldName = It.IsAny<string>(), Times.Never);
			_config.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_config.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public void ItShouldThrowExceptionWhenDestinationIdentityFieldNotExistsInFieldMappings()
		{
			SetupBatchRepository(1);
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			// act
			Func<Task> action = async () => await _synchronizationExecutor
				.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			string errorMessage = "Cannot find destination identifier field in field mappings.";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(errorMessage);
		}

		[Test]
		public void ItShouldThrowExceptionWhenSpecialFieldIsNotFound()
		{
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			List<FieldInfoDto> specialFields =
				_specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(specialFields);
			Func<Task> action = async () => await _synchronizationExecutor
				.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// act
			action();

			// assert
			string expectedMessage = $"Cannot find special field name: {missingSpecialField}";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(expectedMessage);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public async Task ItShouldRunImportApiJobForEachBatch(int numberOfBatches)
		{
			const long jobSize = 12L;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// assert
			_batchRepository.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numberOfBatches));
			_importJob.Verify(x => x.RunAsync(CancellationToken.None), Times.Exactly(numberOfBatches));
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ItShouldCancelImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, tokenSource.Token)
				.ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task ItShouldDisposeImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_importJob.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		[Test]
		public async Task ItShouldProperlyHandleImportSyncException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<ImportFailedException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<ImportFailedException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleAnyImportException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldReturnFailed()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			const int numberOfBatches = 2;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}


		[Test, TestCaseSource(typeof(SynchronizationExecutorTests), nameof(AggregationTestCaseSource))]
		public async Task ItShouldCorrectlyAggregateBatchJobResults((object[] batchJobResultsObject, object expectedResultStatus) testCase)
		{
			Queue<ImportJobResult> batchJobResults = new Queue<ImportJobResult>(testCase.batchJobResultsObject.Select(x => GetJobResult((ExecutionStatus)x, exception: new Exception())));
			SetupBatchRepository(testCase.batchJobResultsObject.Length);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResultAsync());

			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => batchJobResults.Dequeue());

			// act
			ExecutionResult result = await _synchronizationExecutor
				.ExecuteAsync(_config.Object, CancellationToken.None)
				.ConfigureAwait(false);


			result.Status.Should().Be(testCase.expectedResultStatus);
		}

		[Test]
		public async Task ItShouldRespectCancellationBetweenBatches()
		{
			const int batchCount = 2;
			SetupBatchRepository(batchCount);
			SetUpDocumentsTagRepository(ReturnTaggingCompletedResultAsync());

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(CreateSuccessfulResult)
				.Callback(cancellationTokenSource.Cancel);

			// act
			ExecutionResult result = await _synchronizationExecutor
				.ExecuteAsync(_config.Object, cancellationTokenSource.Token)
				.ConfigureAwait(false);


			result.Status.Should().Be(ExecutionStatus.Canceled);
			_importJob.Verify(x => x.RunAsync(cancellationTokenSource.Token), Times.Once);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);

			_batchRepository.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepository.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);
		}

		private void SetUpDocumentsTagRepository(Task<ExecutionResult> executionResult)
		{
			_documentTagRepository
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>())).Returns(executionResult);

			_documentTagRepository
				.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>())).Returns(executionResult);
		}

		private async Task<ExecutionResult> ReturnTaggingCompletedResultAsync()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new ExecutionResult(ExecutionStatus.Completed, "Completed", new Exception());
		}
		private async Task<ExecutionResult> ReturnTaggingCompletedResultAsync(CancellationToken cancellationToken)
		{
			await Task.CompletedTask.ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			return new ExecutionResult(ExecutionStatus.Completed, "Completed", new Exception());
		}

		private async Task<ExecutionResult> ReturnTaggingFailedResultAsync()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			return new ExecutionResult(ExecutionStatus.Failed, "Failed", new Exception());
		}
		private ImportJobResult CreateSuccessfulResult()
		{
			return new ImportJobResult(ExecutionResult.Success(), 1);
		}

		private static ImportJobResult GetJobResult(ExecutionStatus status, string message = null, Exception exception = null, int jobSize = 1)
		{
			return new ImportJobResult(new ExecutionResult(status, message ?? exception?.Message, exception), jobSize);
		}
	}
}