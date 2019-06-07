using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
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
		private Mock<IBatchRepository> _batchRepository;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			_userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_userServiceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);

			_batchRepository = new Mock<IBatchRepository>();
		}

		[Test]
		public async Task ItShouldReturnEmptyArrayWhenNextBatchIsNull()
		{
			// Arrange
			SetupAnyGetNext().ReturnsAsync((IBatch) null);

			var instance = new RelativityExportBatcher(_userServiceFactory.Object, _batchRepository.Object, Guid.Empty, 0, 1);

			// Act
			RelativityObjectSlim[] batch = await instance.GetNextBatchAsync().ConfigureAwait(false);

			// Assert
			batch.Should().NotBeNull().And.BeEmpty();
		}

		[TestCase(123, 12345, 13000)]
		[TestCase(123, 12345, 1000)]
		[TestCase(12345, 123, 1000)]
		public async Task ItShouldReturnAllItemsInBatchInOrder(int startingIndex, int totalItemsCount, int maxResultsSize)
		{
			// Arrange
			var batch = new Mock<IBatch>();
			batch.SetupGet(x => x.StartingIndex).Returns(startingIndex);
			batch.SetupGet(x => x.TotalItemsCount).Returns(totalItemsCount);
			SetupAnyGetNext().ReturnsAsync(batch.Object);

			SetupRetrieveResultsBlock(maxResultsSize);

			var instance = new RelativityExportBatcher(_userServiceFactory.Object, _batchRepository.Object, Guid.Empty, 0, 1);

			// Act
			RelativityObjectSlim[] block = await instance.GetNextBatchAsync().ConfigureAwait(false);

			// Assert
			IEnumerable<int> returnedIds = block.Select(x => x.ArtifactID);
			IEnumerable<int> expectedIds = Enumerable.Range(startingIndex, totalItemsCount);
			returnedIds.Should().BeEquivalentTo(expectedIds);
		}

		private ISetup<IBatchRepository, Task<IBatch>> SetupAnyGetNext()
		{
			return _batchRepository.Setup(x => x.GetNextAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));
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
			return Enumerable.Range(startingIndex, length).Select(x => new RelativityObjectSlim {ArtifactID = x});
		}
	}
}
