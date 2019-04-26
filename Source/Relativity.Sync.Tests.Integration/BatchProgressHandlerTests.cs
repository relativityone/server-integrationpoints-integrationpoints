using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class BatchProgressHandlerTests
	{
		private Mock<IBatch> _batch;
		private FakeImportNotifier _importNotifier;
		private BatchProgressHandler _batchProgressHandler;

		[SetUp]
		public void SetUp()
		{
			_importNotifier = new FakeImportNotifier();
			_batch = new Mock<IBatch>();
			BatchProgressUpdater batchProgressUpdater = new BatchProgressUpdater(_batch.Object, new EmptyLogger());
			_batchProgressHandler = new BatchProgressHandler(_importNotifier, batchProgressUpdater);
		}

		[Test]
		public async Task ItShouldThrottleProgressEvents()
		{
			const int throttleMilliseconds = 100;
			const int numberOfEvents = 5;
			const int delayBetweenEvents = 50;
			const int expectedNumberOfProgressUpdates = 2;
			_batchProgressHandler.Throttle = TimeSpan.FromMilliseconds(throttleMilliseconds);
			
			// act
			for (int i = 0; i < numberOfEvents; i++)
			{
				_importNotifier.RaiseOnProcessProgress(0, 0);
				await Task.Delay(delayBetweenEvents).ConfigureAwait(false);
			}

			// assert
			_batch.Verify(x => x.SetProgressAsync(It.IsAny<double>()), Times.Exactly(expectedNumberOfProgressUpdates));
		}

		[Test]
		public void ItShouldUpdateProgressWhenCompleted()
		{
			const int throttleMilliseconds = 100;
			_batchProgressHandler.Throttle = TimeSpan.FromMilliseconds(throttleMilliseconds);
			const int failedItems = 1;
			const int totalItemsProcessed = 2;

			// act
			_importNotifier.RaiseOnProcessProgress(failedItems, totalItemsProcessed);
			_importNotifier.RaiseOnProcessComplete();

			// assert
			_batch.Verify(x => x.SetFailedItemsCountAsync(failedItems));
			_batch.Verify(x => x.SetTransferredItemsCountAsync(totalItemsProcessed - failedItems));
		}
	}
}