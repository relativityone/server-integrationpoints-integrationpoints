using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class SynchronizationExecutorConstrainsTests
	{
		private CancellationToken _token;
		private ISyncLog _syncLog;

		private Mock<ISynchronizationConfiguration> _synchronizationConfiguration;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
			_syncLog = new EmptyLogger();

			_synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
		}

		[Test]
		[TestCase(BatchStatus.New, ExpectedResult = true)]
		[TestCase(BatchStatus.Cancelled, ExpectedResult = false)]
		[TestCase(BatchStatus.Completed, ExpectedResult = false)]
		[TestCase(BatchStatus.CompletedWithErrors, ExpectedResult = false)]
		[TestCase(BatchStatus.Failed, ExpectedResult = false)]
		[TestCase(BatchStatus.InProgress, ExpectedResult = false)]
		[TestCase(BatchStatus.Started, ExpectedResult = false)]
		public async Task<bool> CanExecuteAsyncGoldFlowTests(BatchStatus testStatus)
		{
			// Arrange
			var lastBatch = new Mock<IBatch>();
			lastBatch.SetupGet(x => x.Status).Returns(testStatus);
			var batchRepository = new Mock<IBatchRepository>();
			batchRepository.Setup(x => x.GetLastAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(lastBatch.Object);

			var synchronizationExecutorConstrains = new SynchronizationExecutorConstrains(batchRepository.Object, _syncLog);

			// Act
			bool actualResult = await synchronizationExecutorConstrains.CanExecuteAsync(_synchronizationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			return actualResult;
		}

		[Test]
		public async Task CanExecuteAsyncReturnsFalseWhenNoBatchesExistTest()
		{
			// Arrange
			var batchRepository = new Mock<IBatchRepository>();
			batchRepository.Setup(x => x.GetLastAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(() => null);

			var synchronizationExecutorConstrains = new SynchronizationExecutorConstrains(batchRepository.Object, _syncLog);

			// Act
			bool actualResult = await synchronizationExecutorConstrains.CanExecuteAsync(_synchronizationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult);
		}

		[Test]
		public void CanExecuteAsyncThrowsWhenGettingLastBatchTest()
		{
			// Arrange
			var batchRepository = new Mock<IBatchRepository>();
			batchRepository.Setup(x => x.GetLastAsync(It.IsAny<int>(), It.IsAny<int>())).Throws<OutOfMemoryException>();

			var synchronizationExecutorConstrains = new SynchronizationExecutorConstrains(batchRepository.Object, _syncLog);

			// Act & Assert
			Assert.ThrowsAsync<OutOfMemoryException>(async () => await synchronizationExecutorConstrains.CanExecuteAsync(_synchronizationConfiguration.Object, _token).ConfigureAwait(false));
		}
	}
}