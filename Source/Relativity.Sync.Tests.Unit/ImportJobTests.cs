using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class ImportJobTests : IDisposable
	{
		private Mock<IItemStatusMonitor> _itemStatusMonitor;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<ISemaphoreSlim> _semaphore;
		private Mock<ISyncImportBulkArtifactJob> _syncImportBulkArtifactJob;

		private ImportJob _importJob;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _JOB_HISTORY_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_itemStatusMonitor = new Mock<IItemStatusMonitor>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_semaphore = new Mock<ISemaphoreSlim>();
			_syncImportBulkArtifactJob = new Mock<ISyncImportBulkArtifactJob>();
			_syncImportBulkArtifactJob.SetupGet(x => x.ItemStatusMonitor).Returns(_itemStatusMonitor.Object);

			_importJob = new ImportJob(_syncImportBulkArtifactJob.Object, _semaphore.Object, _jobHistoryErrorRepository.Object,
				_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldHandleItemLevelError()
		{
			const string identifier = "id";
			const string message = "msg";

			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => x.OnItemLevelError += null, new ItemLevelError(
					identifier,
					message
				));
				_syncImportBulkArtifactJob.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			importJobResult.ExecutionResult.Status.Should().Be(ExecutionStatus.CompletedWithErrors);

			_itemStatusMonitor.Verify(x => x.MarkItemAsFailed(identifier), Times.Once);
			_itemStatusMonitor.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Once);

			_jobHistoryErrorRepository.Verify(x => x.MassCreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.IsAny<IList<CreateJobHistoryErrorDto>>()));
		}

		[Test]
		public async Task ItShouldHandleJobLevelError()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				ImportApiJobStatistics jobReport = CreateJobReport(exception: new InvalidOperationException());
				_syncImportBulkArtifactJob.Raise(x => x.OnFatalException += null, jobReport);
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			importJobResult.ExecutionResult.Status.Should().Be(ExecutionStatus.Failed);

			_itemStatusMonitor.Verify(x => x.MarkReadSoFarAsFailed(), Times.Once);
			_itemStatusMonitor.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Never);

			_jobHistoryErrorRepository.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(dto =>
				dto.ErrorType == ErrorType.Job)));
		}

		[Test]
		public async Task ItShouldReleaseSemaphoreWhenFatalExceptionOccurs()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => x.OnFatalException += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task ItShouldReleaseSemaphoreWhenJobCompletes()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task ItShouldReleaseSemaphoreOnlyOnceWhenFatalExceptionAndCompleteOccurrs()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => x.OnFatalException += null, CreateJobReport());
				_syncImportBulkArtifactJob.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public void ItShouldHandleOnComplete()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Raises(x => x.OnComplete += null, CreateJobReport());

			// act
			Func<Task<ImportJobResult>> action = () => _importJob.RunAsync(CompositeCancellationToken.None);

			// assert
			action.Should().NotThrow();

			_itemStatusMonitor.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenCanceled()
		{
			CancellationTokenSource stopCancellationTokenSource = new CancellationTokenSource();
			stopCancellationTokenSource.Cancel();

			// act
			Func<Task> action = () => _importJob.RunAsync(new CompositeCancellationToken(stopCancellationTokenSource.Token, CancellationToken.None));

			// assert
			action.Should().NotThrow<OperationCanceledException>();
		}
		
		[Test]
		public void ItShouldNotThrowExceptionWhenPausedBeforeExecution()
		{
			CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();
			drainStopCancellationTokenSource.Cancel();

			// act
			Func<Task> action = () => _importJob.RunAsync(new CompositeCancellationToken(CancellationToken.None, drainStopCancellationTokenSource.Token));

			// assert
			action.Should().NotThrow<OperationCanceledException>();
		}
		
		[Test]
		public async Task ItShouldNotExecuteImportJob_WhenCancelledBeforeExecution()
		{
			// Arrange
			CancellationTokenSource stopCancellationTokenSource = new CancellationTokenSource();
			stopCancellationTokenSource.Cancel();

			// Act
			await _importJob.RunAsync(new CompositeCancellationToken(stopCancellationTokenSource.Token, CancellationToken.None));

			// Assert
			_syncImportBulkArtifactJob.Verify(x => x.Execute(), Times.Never);
		}
		
		[Test]
		public async Task ItShouldNotExecuteImportJob_WhenDrainStoppedBeforeExecution()
		{
			// Arrange
			CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();
			drainStopCancellationTokenSource.Cancel();

			// Act
			await _importJob.RunAsync(new CompositeCancellationToken(CancellationToken.None, drainStopCancellationTokenSource.Token));

			// Assert
			_syncImportBulkArtifactJob.Verify(x => x.Execute(), Times.Never);
		}

		[Test]
		public void ItShouldDisposeSemaphoreSlim()
		{
			// act
			_importJob.Dispose();

			// assert
			_semaphore.Verify(x => x.Dispose());
		}

		public void Dispose()
		{
			_importJob?.Dispose();
		}

		private static ImportApiJobStatistics CreateJobReport(int totalItems = 0, int errorItems = 0, long metadataBytes = 0, long fileBytes = 0,
			Exception exception = null)
		{
			return new ImportApiJobStatistics(totalItems, errorItems, metadataBytes, fileBytes, exception);
		}
	}
}