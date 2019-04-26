using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class BatchProgressUpdaterTests
	{
		private Mock<IBatch> _batch;
		private IBatchProgressUpdater _batchProgressUpdater;

		[SetUp]
		public void SetUp()
		{
			_batch = new Mock<IBatch>();
			_batchProgressUpdater = new BatchProgressUpdater(_batch.Object, new EmptyLogger());
		}

		[TestCase(0, 0, 0, 0)]
		[TestCase(0, 5, 5, 100)]
		[TestCase(0, 5, 10, 50)]
		[TestCase(5, 5, 10, 100)]
		[TestCase(2, 3, 10, 50)]
		[TestCase(5, 0, 10, 50)]
		[TestCase(5, 0, 5, 100)]
		[TestCase(1, 5, 13, 46.153846153846153)]
		public async Task ItShouldReportProgress(int failedRecords, int completedRecords, int totalRecords, double expectedProgress)
		{
			_batch.SetupGet(x => x.TotalItemsCount).Returns(totalRecords);

			// act
			await _batchProgressUpdater.UpdateProgressAsync(completedRecords, failedRecords).ConfigureAwait(false);

			// assert
			_batch.Verify(x => x.SetTransferredItemsCountAsync(completedRecords));
			_batch.Verify(x => x.SetFailedItemsCountAsync(failedRecords));
			_batch.Verify(x => x.SetProgressAsync(expectedProgress));
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenSetTransferredItemsCountThrows()
		{
			_batch.Setup(x => x.SetTransferredItemsCountAsync(It.IsAny<int>())).Throws<InvalidOperationException>();

			// act
			Action action = () => _batchProgressUpdater.UpdateProgressAsync(0, 0);

			// assert
			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenSetProgressThrows()
		{
			_batch.Setup(x => x.SetProgressAsync(It.IsAny<double>())).Throws<InvalidOperationException>();

			// act
			Action action = () => _batchProgressUpdater.UpdateProgressAsync(0, 0);

			// assert
			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenSetFailedItemsCountAsyncThrows()
		{
			_batch.Setup(x => x.SetFailedItemsCountAsync(It.IsAny<int>())).Throws<InvalidOperationException>();

			// act
			Action action = () => _batchProgressUpdater.UpdateProgressAsync(0, 0);

			// assert
			action.Should().NotThrow();
		}
	}
}