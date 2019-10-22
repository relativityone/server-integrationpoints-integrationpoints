using System;
using System.Collections.Generic;
using System.Linq;
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
	public class SynchronizationExecutorTests
	{
		private Mock<IBatchRepository> _batchRepositoryMock;
		private Mock<IFieldManager> _fakeFieldManager;
		private Mock<IFieldMappings> _fakeFieldMappings;
		private Mock<IJobStatisticsContainer> _fakeJobStatisticsContainer;
		private Mock<IDocumentTagRepository> _fakeDocumentTagRepository;
		private Mock<IImportJobFactory> _fakeImportJobFactory;
		private Mock<IJobCleanupConfiguration> _jobCleanupConfigurationMock;

		private Mock<Sync.Executors.IImportJob> _fakeImportJob;
		private Mock<ISynchronizationConfiguration> _fakeConfig;

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

		[SetUp]
		public void SetUp()
		{
			_fakeDocumentTagRepository = new Mock<IDocumentTagRepository>();
			_fakeImportJobFactory = new Mock<IImportJobFactory>();
			_batchRepositoryMock = new Mock<IBatchRepository>();
			_fakeJobStatisticsContainer = new Mock<IJobStatisticsContainer>();
			_fakeFieldManager = new Mock<IFieldManager>();
			_fakeFieldMappings = new Mock<IFieldMappings>();
			_fakeDocumentTagRepository = new Mock<IDocumentTagRepository>();
			_fakeConfig = new Mock<ISynchronizationConfiguration>();
			_jobCleanupConfigurationMock = new Mock<IJobCleanupConfiguration>();

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

			_fakeImportJob = new Mock<Sync.Executors.IImportJob>();
			_fakeImportJobFactory.Setup(x => x.CreateImportJobAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).ReturnsAsync(_fakeImportJob.Object);

			_fakeFieldManager.Setup(x => x.GetSpecialFields()).Returns(_specialFields);

			_sut = new SynchronizationExecutor(_fakeImportJobFactory.Object, _batchRepositoryMock.Object, 
				_fakeDocumentTagRepository.Object,_fakeFieldManager.Object, _fakeFieldMappings.Object, _fakeJobStatisticsContainer.Object, 
				_jobCleanupConfigurationMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettings()
		{
			SetupBatchRepository(1);
			_fakeConfig.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_fakeConfig.VerifySet(x => x.FolderPathSourceFieldName = _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}


		[Test]
		public async Task Execute_ShouldCancelTaggingResultTest()
		{
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_fakeConfig.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync(tokenSource.Token);
			SetUpDocumentsTagRepository(executionResult);

			//Act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			//Assert
			result.Message.Should()
				.Be("Tagging synchronized documents in workspace was interrupted due to the job being canceled.");
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldCatchExceptionTest()
		{
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_fakeConfig.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = null;

			SetUpDocumentsTagRepository(executionResult);

			//Act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			//Assert
			result.Message.Should()
				.Be("Unexpected exception occurred while tagging synchronized documents in workspace.");
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetImportApiSettingsExceptFolderInfo()
		{
			//Arrange
			const long jobSize = 12L;
			SetupBatchRepository(1);
			_fakeConfig.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_fakeConfig.VerifySet(x => x.FolderPathSourceFieldName = It.IsAny<string>(), Times.Never);
			_fakeConfig.VerifySet(x => x.FileSizeColumn = _NATIVE_FILE_SIZE_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.NativeFilePathSourceFieldName = _NATIVE_FILE_LOCATION_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.FileNameColumn = _NATIVE_FILE_FILENAME_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.OiFileTypeColumnName = _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, Times.Once);
			_fakeConfig.VerifySet(x => x.SupportedByViewerColumn = _SUPPORTED_BY_VIEWER_DISPLAY_NAME, Times.Once);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenDestinationIdentityFieldNotExistsInFieldMappings()
		{
			SetupBatchRepository(1);
			_fakeFieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			// act
			Func<Task> action = async () => await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			string errorMessage = "Cannot find destination identifier field in field mappings.";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(errorMessage);
		}

		[Test]
		public void Execute_ShouldThrowException_WhenSpecialFieldIsNotFound()
		{
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			List<FieldInfoDto> specialFields = _specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fakeFieldManager.Setup(x => x.GetSpecialFields()).Returns(specialFields);
			Func<Task> action = async () => await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

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
			const long jobSize = 12L;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());
			ImportJobResult importJob = new ImportJobResult(ExecutionResult.Success(), jobSize);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importJob);

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();

			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_batchRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numberOfBatches));
			_fakeImportJob.Verify(x => x.RunAsync(CancellationToken.None), Times.Exactly(numberOfBatches));
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task Execute_ShouldCancelImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, tokenSource.Token).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task Execute_ShouldDisposeImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingCompletedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_fakeImportJob.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportSyncException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<ImportFailedException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<ImportFailedException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleAnyImportException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldReturnFailed()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenCompletedSuccessfully()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		[Test]
		public async Task Execute_ShouldSetExecutionResultForJobCleanupConfiguration_WhenFailed()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_fakeImportJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSuccessfulResult());

			Task<ExecutionResult> executionResult = ReturnTaggingFailedResultAsync();
			SetUpDocumentsTagRepository(executionResult);

			// act
			ExecutionResult result = await _sut.ExecuteAsync(_fakeConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			_jobCleanupConfigurationMock.VerifySet(x => x.SynchronizationExecutionResult = result);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);

			_batchRepositoryMock.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);
		}

		private void SetUpDocumentsTagRepository(Task<ExecutionResult> executionResult)
		{
			_fakeDocumentTagRepository
				.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IEnumerable<int>>(),
					It.IsAny<CancellationToken>())).Returns(executionResult);

			_fakeDocumentTagRepository
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
	}
}