using System;
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

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class SynchronizationExecutorTests
	{
		private Mock<IImportJobFactory> _importJobFactory;
		private Mock<IBatchRepository> _batchRepository;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IDateTime> _dateTime;
		private Mock<Relativity.Sync.Executors.IImportJob> _importJob;

		private SynchronizationExecutor _synchronizationExecutor;

		[SetUp]
		public void SetUp()
		{
			_importJobFactory = new Mock<IImportJobFactory>();
			_batchRepository = new Mock<IBatchRepository>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_dateTime = new Mock<IDateTime>();
			_synchronizationExecutor = new SynchronizationExecutor(_importJobFactory.Object, _batchRepository.Object, _syncMetrics.Object, _dateTime.Object, new EmptyLogger());
		}
		
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public async Task ItShouldRunImportApiJobForEachBatch(int numberOfBatches)
		{
			SetupImportJobFactory(numberOfBatches);

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
			SetupImportJobFactory(numberOfBatches);

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
			SetupImportJobFactory(numberOfBatches);
			
			// act
			await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_importJob.Verify(x => x.Dispose(), Times.Exactly(numberOfBatches));
		}

		[Test]
		public async Task ItShouldProperlyHandleSyncException()
		{
			const int numberOfBatches = 1;
			SetupImportJobFactory(numberOfBatches);

			SyncException syncException = new SyncException(string.Empty, new InvalidOperationException());
			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws(syncException);

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Fatal exception occurred while executing import job.");
			result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldProperlyHandleAnyException()
		{
			const int numberOfBatches = 1;
			SetupImportJobFactory(numberOfBatches);

			_importJob.Setup(x => x.RunAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _synchronizationExecutor.ExecuteAsync(Mock.Of<ISynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			result.Message.Should().Be("Unexpected exception occurred while executing import job.");
			result.Exception.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		private void SetupImportJobFactory(int numberOfBatches)
		{
			_batchRepository.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new int[numberOfBatches].ToList());
			_importJob = new Mock<Sync.Executors.IImportJob>();
			_importJobFactory.Setup(x => x.CreateImportJob(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>())).Returns(_importJob.Object);
		}
	}
}