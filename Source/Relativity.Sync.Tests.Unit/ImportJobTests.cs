using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Relativity.DataReaderClient;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

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
				_syncImportBulkArtifactJob.Raise(x => ((IImportNotifier) x).OnComplete += null, CreateJobReport());
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

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
				JobReport jobReport = CreateJobReport();
				PropertyInfo fatalException = jobReport.GetType().GetProperty(nameof(jobReport.FatalException));
				InvalidOperationException ex = new InvalidOperationException();
				fatalException?.SetValue(jobReport, ex, BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);

				_syncImportBulkArtifactJob.Raise(x => x.OnFatalException += null, jobReport);
			});

			// act
			ImportJobResult importJobResult = await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

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
			await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task ItShouldReleaseSemaphoreWhenJobCompletes()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => ((IImportNotifier) x).OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public async Task ItShouldReleaseSemaphoreOnlyOnceWhenFatalExceptionAndCompleteOccurrs()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
			{
				_syncImportBulkArtifactJob.Raise(x => x.OnFatalException += null, CreateJobReport());
				_syncImportBulkArtifactJob.Raise(x => ((IImportNotifier) x).OnComplete += null, CreateJobReport());
			});

			// act
			await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			_semaphore.Verify(x => x.Release(), Times.Once);
		}

		[Test]
		public void ItShouldHandleOnComplete()
		{
			_syncImportBulkArtifactJob.Setup(x => x.Execute()).Raises(x => ((IImportNotifier) x).OnComplete += null, CreateJobReport());

			// act
			Func<Task<ImportJobResult>> action = async () => await _importJob.RunAsync(CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().NotThrow();

			_itemStatusMonitor.Verify(x => x.MarkReadSoFarAsSuccessful(), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenCanceled()
		{
			CancellationTokenSource token = new CancellationTokenSource();
			token.Cancel();

			// act
			Func<Task> action = async () => await _importJob.RunAsync(token.Token).ConfigureAwait(false);

			// assert
			action.Should().NotThrow<OperationCanceledException>();
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

		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
		}

	}
}