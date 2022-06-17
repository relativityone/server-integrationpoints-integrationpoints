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
		private Mock<IItemStatusMonitor> _itemStatusMonitorMock;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepositoryMock;
		private Mock<ISemaphoreSlim> _semaphoreMock;
		private Mock<ISyncImportBulkArtifactJob> _syncImportBulkArtifactJobMock;

		private ImportJob _importJob;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _JOB_HISTORY_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_itemStatusMonitorMock = new Mock<IItemStatusMonitor>();
			_jobHistoryErrorRepositoryMock = new Mock<IJobHistoryErrorRepository>();
			_semaphoreMock = new Mock<ISemaphoreSlim>();
			_syncImportBulkArtifactJobMock = new Mock<ISyncImportBulkArtifactJob>();
			_syncImportBulkArtifactJobMock.SetupGet(x => x.ItemStatusMonitor).Returns(_itemStatusMonitorMock.Object);

			_importJob = new ImportJob(_syncImportBulkArtifactJobMock.Object, _semaphoreMock.Object, _jobHistoryErrorRepositoryMock.Object,
				_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, new EmptyLogger());
		}

		[Test]
		public async Task RunAsync_ShouldHandleItemLevelError()
		{
			const string identifier = "id";
			const string message = "msg";

			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJobMock.Raise(x => x.OnItemLevelError += null, new ItemLevelError(
					identifier,
					message
				));
				_syncImportBulkArtifactJobMock.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			importJobResult.ExecutionResult.Status.Should().Be(ExecutionStatus.CompletedWithErrors);

			_itemStatusMonitorMock.Verify(x => x.MarkItemAsFailed(identifier), Times.Once);
			_itemStatusMonitorMock.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Once);

			_jobHistoryErrorRepositoryMock.Verify(x => x.MassCreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.IsAny<IList<CreateJobHistoryErrorDto>>()));
		}

		[Test]
		public async Task RunAsync_ShouldHandlePaused_WhenItemLevelErrorsOccurred()
		{
			const string identifier = "id";
			const string message = "msg";

			CancellationTokenSource drainStopTokenSource = new CancellationTokenSource();
			CompositeCancellationToken token = ComposeToken(CancellationToken.None, drainStopTokenSource.Token);
			
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				drainStopTokenSource.Cancel();

				_syncImportBulkArtifactJobMock.Raise(x => x.OnItemLevelError += null, new ItemLevelError(
					identifier,
					message
				));
				_syncImportBulkArtifactJobMock.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(token).ConfigureAwait(false);

			// assert
			importJobResult.ExecutionResult.Status.Should().Be(ExecutionStatus.Paused);
		}

		[Test]
		public async Task RunAsync_ShouldHandleJobLevelError()
		{
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				ImportApiJobStatistics jobReport = CreateJobReport(exception: new InvalidOperationException());
				_syncImportBulkArtifactJobMock.Raise(x => x.OnFatalException += null, jobReport);
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			importJobResult.ExecutionResult.Status.Should().Be(ExecutionStatus.Failed);

			_itemStatusMonitorMock.Verify(x => x.MarkReadSoFarAsFailed(), Times.Once);
			_itemStatusMonitorMock.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Never);

			_jobHistoryErrorRepositoryMock.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _JOB_HISTORY_ARTIFACT_ID, It.Is<CreateJobHistoryErrorDto>(dto =>
				dto.ErrorType == ErrorType.Job)));
		}

		[Test]
		public async Task RunAsync_ShouldReleaseSemaphoreWhenFatalExceptionOccurs()
		{
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJobMock.Raise(x => x.OnFatalException += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphoreMock.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task RunAsync_ShouldReleaseSemaphoreWhenJobCompletes()
		{
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJobMock.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphoreMock.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task RunAsync_ShouldReleaseSemaphoreOnlyOnceWhenFatalExceptionAndCompleteOccurrs()
		{
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJobMock.Raise(x => x.OnFatalException += null, CreateJobReport());
				_syncImportBulkArtifactJobMock.Raise(x => x.OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphoreMock.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public void RunAsync_ShouldHandleOnComplete()
		{
			_syncImportBulkArtifactJobMock.Setup(x => x.Execute()).Raises(x => x.OnComplete += null, CreateJobReport());

			// act
			Func<Task<ImportJobResult>> action = () => _importJob.RunAsync(CompositeCancellationToken.None);

			// assert
			action.Should().NotThrow();

			_itemStatusMonitorMock.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Once);
		}

		[Test]
		public void RunAsync_ShouldNotThrowExceptionWhenCanceled()
		{
			CancellationTokenSource stopCancellationTokenSource = new CancellationTokenSource();
			stopCancellationTokenSource.Cancel();

			// act
			Func<Task> action = () => _importJob.RunAsync(ComposeToken(stopCancellationTokenSource.Token, CancellationToken.None));

			// assert
			action.Should().NotThrow<OperationCanceledException>();
		}
		
		[Test]
		public void RunAsync_ShouldNotThrowExceptionWhenPausedBeforeExecution()
		{
			CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();
			drainStopCancellationTokenSource.Cancel();

			// act
			Func<Task> action = () => _importJob.RunAsync(ComposeToken(CancellationToken.None, drainStopCancellationTokenSource.Token));

			// assert
			action.Should().NotThrow<OperationCanceledException>();
		}
		
		[Test]
		public async Task RunAsync_ShouldNotExecuteImportJob_WhenCancelledBeforeExecution()
		{
			// Arrange
			CancellationTokenSource stopCancellationTokenSource = new CancellationTokenSource();
			stopCancellationTokenSource.Cancel();

			// Act
			await _importJob.RunAsync(ComposeToken(stopCancellationTokenSource.Token, CancellationToken.None));

			// Assert
			_syncImportBulkArtifactJobMock.Verify(x => x.Execute(), Times.Never);
		}
		
		[Test]
		public async Task RunAsync_ShouldNotExecuteImportJob_WhenDrainStoppedBeforeExecution()
		{
			// Arrange
			CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();
			drainStopCancellationTokenSource.Cancel();

			// Act
			await _importJob.RunAsync(ComposeToken(CancellationToken.None, drainStopCancellationTokenSource.Token));

			// Assert
			_syncImportBulkArtifactJobMock.Verify(x => x.Execute(), Times.Never);
		}

		[Test]
		public void Dispose_ShouldDisposeSemaphoreSlim()
		{
			// act
			_importJob.Dispose();

			// assert
			_semaphoreMock.Verify(x => x.Dispose());
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

		private CompositeCancellationToken ComposeToken(CancellationToken stopCancellationToken, CancellationToken drainStopCancellationToken)
        {
			return new CompositeCancellationToken(stopCancellationToken, drainStopCancellationToken, new EmptyLogger());
        }
	}
}