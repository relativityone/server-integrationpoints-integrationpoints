using System;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class BatchProgressHandlerTests
	{
		private Mock<IBatch> _batch;
		private Mock<IDateTime> _dateTime;
		private FakeImportNotifier _importNotifier;

		[SetUp]
		public void SetUp()
		{
			_batch = new Mock<IBatch>();
			_dateTime = new Mock<IDateTime>();
			_importNotifier = new FakeImportNotifier();
			BatchProgressUpdater batchProgressUpdater = new BatchProgressUpdater(new EmptyLogger());
			BatchProgressHandler batchProgressHandler = new BatchProgressHandler(_batch.Object, batchProgressUpdater, _dateTime.Object);
			_importNotifier.OnComplete += batchProgressHandler.HandleProcessComplete;
			_importNotifier.OnProcessProgress += batchProgressHandler.HandleProcessProgress;
		}

		[TestCase(0, 0, 0)]
		[TestCase(0, 123, 0)]
		[TestCase(1, 0, 1)]
		[TestCase(1, 500, 1)]
		[TestCase(2, 500, 1)]
		[TestCase(2, 1000, 2)]
		[TestCase(3, 500, 2)]
		[TestCase(4, 500, 2)]
		[TestCase(4, 1000, 4)]
		[TestCase(5, 500, 3)]
		[TestCase(20, 500, 10)]
		public void ItShouldThrottleProgressEvents(int numberOfEvents, int delayBetweenEvents, int expectedNumberOfProgressUpdates)
		{
			DateTime now = DateTime.Now;

			// act
			for (int i = 0; i < numberOfEvents; i++)
			{
				now += TimeSpan.FromMilliseconds(delayBetweenEvents);
				_dateTime.SetupGet(x => x.Now).Returns(now);

				_importNotifier.RaiseOnProcessProgress(0, 0);
			}

			// assert
			_batch.Verify(x => x.SetProgressAsync(It.IsAny<double>()), Times.Exactly(expectedNumberOfProgressUpdates));
		}

		[Test]
		public void ItShouldUpdateProgressWhenCompleted()
		{
			const int failedItems = 1;
			const int totalItemsProcessed = 2;

			// act
			_importNotifier.RaiseOnProcessComplete(failedItems, totalItemsProcessed);

			// assert
			_batch.Verify(x => x.SetFailedItemsCountAsync(failedItems));
			_batch.Verify(x => x.SetTransferredItemsCountAsync(totalItemsProcessed - failedItems));
		}
	}
}