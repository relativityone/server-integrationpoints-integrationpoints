using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
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
		private Mock<IDateTime> _dateTime;
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspaceTagRepository;
		private Mock<IFieldManager> _fieldManager;

		private Mock<IImportJobFactory> _importJobFactory;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<Sync.Executors.IImportJob> _importJob;

		private SynchronizationExecutor _synchronizationExecutor;

		private const string _FOLDER_PATH_COLUMN = "folder path";
		private const string _NATIVE_FILENAME_COLUMN = "native file name";
		private const string _NATIVE_FILESIZE_COLUMN = "native file size";
		private const string _NATIVE_LOCATION_COLUMN = "native location";
		private const string _NATIVE_TYPE_COLUMN = "native type";
		private const string _SUPPORTED_BY_VIEWER_COLUMN = "supported by viewer";

		private readonly List<FieldInfoDto> _specialFields = new List<FieldInfoDto>()
		{
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.FolderPath, DisplayName = _FOLDER_PATH_COLUMN},
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.NativeFileSize, DisplayName = _NATIVE_FILESIZE_COLUMN},
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.NativeFileLocation, DisplayName = _NATIVE_LOCATION_COLUMN},
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.NativeFileFilename, DisplayName = _NATIVE_FILENAME_COLUMN},
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.RelativityNativeType, DisplayName = _NATIVE_TYPE_COLUMN},
			new FieldInfoDto(){SpecialFieldType = SpecialFieldType.SupportedByViewer, DisplayName = _SUPPORTED_BY_VIEWER_COLUMN}
		};
		
		[SetUp]
		public void SetUp()
		{
			_importJobFactory = new Mock<IImportJobFactory>();
			_batchRepository = new Mock<IBatchRepository>();
			_destinationWorkspaceTagRepository = new Mock<IDestinationWorkspaceTagRepository>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_dateTime = new Mock<IDateTime>();
			_fieldManager = new Mock<IFieldManager>();

			_importJob = new Mock<Sync.Executors.IImportJob>();
			_importJobFactory.Setup(x => x.CreateImportJob(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>())).Returns(_importJob.Object);
			
			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(_specialFields);

			_synchronizationExecutor = new SynchronizationExecutor(_importJobFactory.Object, _batchRepository.Object, _destinationWorkspaceTagRepository.Object,
				_syncMetrics.Object, _dateTime.Object, _fieldManager.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldSetImportApiSettings()
		{
			SetupBatchRepository(1);
			
			Mock<ISynchronizationConfiguration> syncConfig = new Mock<ISynchronizationConfiguration>();

			// act
			await _synchronizationExecutor.ExecuteAsync(syncConfig.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			syncConfig.Verify(x => x.SetImportSettings(It.Is<ImportSettingsDto>(settings => 
				settings.FolderPathSourceFieldName == _FOLDER_PATH_COLUMN &&
				settings.FileSizeColumn == _NATIVE_FILESIZE_COLUMN &&
				settings.NativeFilePathSourceFieldName == _NATIVE_LOCATION_COLUMN &&
				settings.FileNameColumn == _NATIVE_FILENAME_COLUMN &&
				settings.OiFileTypeColumnName == _NATIVE_TYPE_COLUMN &&
				settings.SupportedByViewerColumn == _SUPPORTED_BY_VIEWER_COLUMN
				)));
		}

		[Test]
		public void ItShouldThrowExceptionWhenSpecialFieldIsNotFound()
		{
			SpecialFieldType missingSpecialField = SpecialFieldType.NativeFileSize;
			List<FieldInfoDto> specialFields = _specialFields.Where(x => x.SpecialFieldType != missingSpecialField).ToList();
			_fieldManager.Setup(x => x.GetSpecialFields()).Returns(specialFields);
			Func<Task> action = async () => await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

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

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

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
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), tokenSource.Token).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Canceled);
		}

		[Test]
		public async Task ItShouldDisposeImportJob()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);
			
			// act
			await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_importJob.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		[Test]
		public async Task ItShouldProperlyHandleImportSyncException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			SyncException syncException = new SyncException(string.Empty, new InvalidOperationException());
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws(syncException);

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleAnyImportException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing import job.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleTagDocumentException()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			_destinationWorkspaceTagRepository.Setup(x => x.TagDocumentsAsync(
				It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while tagging synchronized documents in source workspace.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleImportAndTagDocumentExceptions()
		{
			const int numberOfBatches = 1;
			SetupBatchRepository(numberOfBatches);

			const int numberOfExceptions = 2;
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();
			_destinationWorkspaceTagRepository.Setup(x => x.TagDocumentsAsync(
				It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IList<int>>(), It.IsAny<CancellationToken>())).Throws<NotAuthorizedException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing import job. Unexpected exception occurred while tagging synchronized documents in source workspace.");
			result.Exception.Should().BeOfType<AggregateException>();
			ReadOnlyCollection<Exception> exceptions = ((AggregateException) result.Exception).InnerExceptions;
			exceptions.Should().NotBeNullOrEmpty();
			exceptions.Should().HaveCount(numberOfExceptions);
			exceptions[0].Should().BeOfType<InvalidOperationException>();
			exceptions[1].Should().BeOfType<NotAuthorizedException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		private void SetupBatchRepository(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);
			var batch = new Mock<IBatch>();
			batch.Setup(x => x.GetItemArtifactIds(It.IsAny<Guid>())).ReturnsAsync(batches);

			_batchRepository.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepository.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);
		}
	}
}