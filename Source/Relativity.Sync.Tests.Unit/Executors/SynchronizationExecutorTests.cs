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

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class SynchronizationExecutorTests
	{
		private Mock<IBatchRepository> _batchRepository;
		private Mock<IDateTime> _dateTime;
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspaceTagRepository;
		private Mock<IImportJobFactory> _importJobFactory;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<Sync.Executors.IImportJob> _importJob;

		private SynchronizationExecutor _synchronizationExecutor;
		
		[SetUp]
		public void SetUp()
		{
			_importJobFactory = new Mock<IImportJobFactory>();
			_batchRepository = new Mock<IBatchRepository>();
			_destinationWorkspaceTagRepository = new Mock<IDestinationWorkspaceTagRepository>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_dateTime = new Mock<IDateTime>();
			_synchronizationExecutor = new SynchronizationExecutor(_batchRepository.Object, _dateTime.Object, _destinationWorkspaceTagRepository.Object,
				_importJobFactory.Object, new EmptyLogger(), _syncMetrics.Object);
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
		public async Task ItShouldProperlyHandleImportSyncException()
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
		public async Task ItShouldProperlyHandleAnyImportException()
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

		[Test]
		public async Task ItShouldProperlyHandleTagDocumentException()
		{
			const int numberOfBatches = 1;
			SetupImportJobFactory(numberOfBatches);

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
			SetupImportJobFactory(numberOfBatches);

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

		private void SetupImportJobFactory(int numberOfBatches)
		{
			IEnumerable<int> batches = Enumerable.Repeat(1, numberOfBatches);
			var batch = new Mock<IBatch>();
			batch.Setup(x => x.GetItemArtifactIds(It.IsAny<Guid>())).ReturnsAsync(batches);

			_batchRepository.Setup(x => x.GetAllNewBatchesIdsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batches);
			_batchRepository.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((new Mock<IBatch>()).Object);

			_importJob = new Mock<Sync.Executors.IImportJob>();
			_importJobFactory.Setup(x => x.CreateImportJob(It.IsAny<ISynchronizationConfiguration>(), It.IsAny<IBatch>())).Returns(_importJob.Object);
		}
	}
}