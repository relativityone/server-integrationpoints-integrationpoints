using kCura.Config;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
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
			_batch = new Mock<IBatch>();
			_importNotifier = new FakeImportNotifier();
			_batchProgressHandler = new BatchProgressHandler(_batch.Object, _importNotifier);
		}

		[TestCase(0, 0, 0, 0)]
		[TestCase(0, 5, 5, 100)]
		[TestCase(0, 5, 10, 50)]
		[TestCase(5, 5, 10, 100)]
		[TestCase(2, 3, 10, 50)]
		[TestCase(5, 0, 10, 50)]
		[TestCase(5, 0, 5, 100)]
		public void ItShouldReportProgress(int failedItems, int completedItems, int totalItems, double expectedProgress)
		{
			_batch.SetupGet(x => x.TotalItemsCount).Returns(totalItems);

			// act
			_importNotifier.RaiseOnProcessProgress(failedItems, failedItems + completedItems);

			// assert
			_batch.Verify(x => x.SetTransferredItemsCountAsync(completedItems));
			_batch.Verify(x => x.SetFailedItemsCountAsync(failedItems));
			_batch.Verify(x => x.SetProgressAsync(expectedProgress));
		}
	}
}