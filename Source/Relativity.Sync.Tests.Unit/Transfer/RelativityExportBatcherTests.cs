using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class RelativityExportBatcherTests
	{
		private Mock<IObjectManager> _objectManager;
		private Mock<ISourceServiceFactoryForUser> _userServiceFactory;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			_userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public void ItShouldReturnAllItemsInOneBlock()
		{
			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.StartingIndex).Returns(0);
			const int totalItemsCount = 10;
			batch.SetupGet(x => x.TotalItemsCount).Returns(totalItemsCount);
			SetupRetrieveResultsBlock(totalItemsCount);
			RelativityExportBatcher exportBatcher = new RelativityExportBatcher(_userServiceFactory.Object, batch.Object, Guid.Empty, 0);

			// act
			Task<RelativityObjectSlim[]> firstResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();
			Task<RelativityObjectSlim[]> secondResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();

			// assert
			firstResultsBlock.Result.Length.Should().Be(totalItemsCount);
			secondResultsBlock.Result.Length.Should().Be(0);
		}

		[Test]
		public void ItShouldReturnItemsInTwoBlocks()
		{
			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.StartingIndex).Returns(0);
			const int totalItemsCount = 10;
			const int maxResultsBlockSize = 7;
			batch.SetupGet(x => x.TotalItemsCount).Returns(totalItemsCount);
			RelativityExportBatcher exportBatcher = new RelativityExportBatcher(_userServiceFactory.Object, batch.Object, Guid.Empty, 0);

			// act
			SetupRetrieveResultsBlock(maxResultsBlockSize);
			Task<RelativityObjectSlim[]> firstResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();
			SetupRetrieveResultsBlock(totalItemsCount - maxResultsBlockSize);
			Task<RelativityObjectSlim[]> secondResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();

			// assert
			firstResultsBlock.Result.Length.Should().Be(maxResultsBlockSize);
			secondResultsBlock.Result.Length.Should().Be(totalItemsCount - maxResultsBlockSize);
		}


		[Test]
		public async Task ShouldNotCallObjectManagerWhenRemainingItemsIsZero()
		{
			const int totalItemsCount = 10;


			SetupRetrieveResultsBlock(totalItemsCount);

			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.StartingIndex).Returns(0);
			batch.SetupGet(x => x.TotalItemsCount).Returns(0);

			RelativityExportBatcher batcher = new RelativityExportBatcher(_userServiceFactory.Object, batch.Object, Guid.Empty, 0);

			RelativityObjectSlim[] batches = await batcher.GetNextItemsFromBatchAsync().ConfigureAwait(false);

			_objectManager.Verify(x => x.RetrieveResultsBlockFromExportAsync(0, Guid.Empty, 0, It.IsAny<int>()), Times.Never);
			Assert.IsFalse(batches.Any());
		}

		private void SetupRetrieveResultsBlock(int maxResultSize)
		{
			_objectManager.Setup(x =>
				x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())
			).ReturnsAsync<int, Guid, int, int, IObjectManager, RelativityObjectSlim[]>((a, b, len, ind) =>
				CreateBatch(ind, len).Take(maxResultSize).ToArray());
		}

		private static IEnumerable<RelativityObjectSlim> CreateBatch(int startingIndex, int length)
		{
			return Enumerable.Range(startingIndex, length).Select(x => new RelativityObjectSlim { ArtifactID = x });
		}
	}
}
