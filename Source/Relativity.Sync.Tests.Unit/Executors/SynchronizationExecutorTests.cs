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
		private Mock<IBatchRepository> _batchRepository;
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspaceTagRepository;
		private Mock<IFieldManager> _fieldManager;
		private Mock<IFieldMappings> _fieldMappings;

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

		[SetUp]
		public void SetUp()
		{
			var importJobFactory = new Mock<IImportJobFactory>();
			_batchRepository = new Mock<IBatchRepository>();
			_destinationWorkspaceTagRepository = new Mock<IDestinationWorkspaceTagRepository>();
			_fieldManager = new Mock<IFieldManager>();
			_fieldMappings = new Mock<IFieldMappings>();
			Mock<IJobHistoryErrorRepository> jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_config = new Mock<ISynchronizationConfiguration>();
			_config.SetupGet(x => x.ImportSettings).Returns(new ImportSettingsDto());
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
			importJobFactory.Setup(x => x.CreateImportJobAsync(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>())).ReturnsAsync(_importJob.Object);

			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(_specialFields);

			_synchronizationExecutor = new SynchronizationExecutor(importJobFactory.Object, _batchRepository.Object, _destinationWorkspaceTagRepository.Object, null,
				_fieldManager.Object, _fieldMappings.Object, jobHistoryErrorRepository.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldSetImportApiSettings()
		{
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ExecutionResult.Success);

			// act
			await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(_FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, _config.Object.ImportSettings.FolderPathSourceFieldName);
			Assert.AreEqual(_NATIVE_FILE_SIZE_DISPLAY_NAME, _config.Object.ImportSettings.FileSizeColumn);
			Assert.AreEqual(_NATIVE_FILE_LOCATION_DISPLAY_NAME, _config.Object.ImportSettings.NativeFilePathSourceFieldName);
			Assert.AreEqual(_NATIVE_FILE_FILENAME_DISPLAY_NAME, _config.Object.ImportSettings.FileNameColumn);
			Assert.AreEqual(_RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, _config.Object.ImportSettings.OiFileTypeColumnName);
			Assert.AreEqual(_SUPPORTED_BY_VIEWER_DISPLAY_NAME, _config.Object.ImportSettings.SupportedByViewerColumn);
		}

		[Test]
		public async Task ItShouldSetImportApiSettingsExceptFolderInfo()
		{
			SetupBatchRepository(1);
			_config.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ExecutionResult.Success);

			// act
			await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(null, _config.Object.ImportSettings.FolderPathSourceFieldName);
			Assert.AreEqual(_NATIVE_FILE_SIZE_DISPLAY_NAME, _config.Object.ImportSettings.FileSizeColumn);
			Assert.AreEqual(_NATIVE_FILE_LOCATION_DISPLAY_NAME, _config.Object.ImportSettings.NativeFilePathSourceFieldName);
			Assert.AreEqual(_NATIVE_FILE_FILENAME_DISPLAY_NAME, _config.Object.ImportSettings.FileNameColumn);
			Assert.AreEqual(_RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, _config.Object.ImportSettings.OiFileTypeColumnName);
			Assert.AreEqual(_SUPPORTED_BY_VIEWER_DISPLAY_NAME, _config.Object.ImportSettings.SupportedByViewerColumn);
		}

		[Test]
		public void ItShouldThrowExceptionWhenDestinationIdentityFieldNotExistsInFieldMappings()
		{
			SetupBatchRepository(1);
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			// act
			Func<Task> action = async () => await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			string errorMessage = "Cannot find destination identifier field in field mappings.";
			action.Should().Throw<SyncException>().Which.Message.Should().Be(errorMessage);
		}

		[Test]
		public void ItShouldThrowExceptionWhenSpecialFieldIsNotFound()
		{
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			List<FieldInfoDto> specialFields = _specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(specialFields);
			Func<Task> action = async () => await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

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
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ExecutionResult.Success);

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

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
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, tokenSource.Token).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task ItShouldDisposeImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ExecutionResult.Success);

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
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

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
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleTagDocumentException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ExecutionResult.Success);

			_importJob.Setup(x => x.GetPushedDocumentArtifactIds()).ReturnsAsync(new[] { 1 });
			_destinationWorkspaceTagRepository.Setup(x => x.TagDocumentsAsync(
				It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while tagging synchronized documents in source workspace.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing synchronization.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);

			_batchRepository.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepository.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);
		}
	}
}