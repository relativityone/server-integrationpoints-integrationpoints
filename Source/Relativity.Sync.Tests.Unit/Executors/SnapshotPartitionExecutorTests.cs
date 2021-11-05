using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class SnapshotPartitionExecutorTests
	{
		private SnapshotPartitionExecutor _instance;

		private Mock<IBatchRepository> _batchRepository;
		private Mock<ISnapshotPartitionConfiguration> _configuration;

		private const int _WORKSPACE_ID = 589632;
		private const int _SYNC_CONF_ID = 214563;

		[SetUp]
		public void SetUp()
		{
			_batchRepository = new Mock<IBatchRepository>();
			_configuration = new Mock<ISnapshotPartitionConfiguration>();
			_configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			_configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);

			_instance = new SnapshotPartitionExecutor(_batchRepository.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldReturnFailureWhenCannotReadLastBatch()
		{
			_batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).Throws<InvalidOperationException>();

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Failed);
			result.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldReturnFailureWhenUnableToCreateBatch()
		{
			const int ten = 10;
			_configuration.Setup(x => x.BatchSize).Returns(1);
			_configuration.Setup(x => x.TotalRecordsCount).Returns(ten);

			IBatch batch = null;
			_batchRepository.SetupSequence(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batch).Throws<InvalidOperationException>();
			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Failed);
			result.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldCreateBatchesWhenTheyDoNotExist()
		{
			const int items = 10;
			_batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync((IBatch) null);

			_configuration.Setup(x => x.BatchSize).Returns(1);
			_configuration.Setup(x => x.TotalRecordsCount).Returns(items);

			_batchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch) null);

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);

			for (int i = 0; i < items; i++)
			{
				int index = i;
				_batchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), 1, index));
			}
		}

		[Test]
		public async Task ItShouldAddMissingBatches()
		{
			const int lastStartingIndex = 5;
			const int lastBatchSize = 10;
			const int totalItems = 40;
			const int batchSize = 30;

			const int indexToStartFrom = 15;
			const int itemsLeft = 25;

			Mock<IBatch> lastBatch = new Mock<IBatch>();
			lastBatch.Setup(x => x.StartingIndex).Returns(lastStartingIndex);
			lastBatch.Setup(x => x.TotalDocumentsCount).Returns(lastBatchSize);

			_batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(lastBatch.Object);

			_configuration.Setup(x => x.BatchSize).Returns(batchSize);
			_configuration.Setup(x => x.TotalRecordsCount).Returns(totalItems);

			_batchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch) null);

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);

			_batchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), itemsLeft, indexToStartFrom));
		}

		[Test]
		public async Task ItShouldSucceedWhenNoMoreBatchesIsRequired()
		{
			const int ten = 10;
			Mock<IBatch> lastBatch = new Mock<IBatch>();
			lastBatch.Setup(x => x.StartingIndex).Returns(ten);
			lastBatch.Setup(x => x.TotalDocumentsCount).Returns(ten);

			_batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(lastBatch.Object);

			_configuration.Setup(x => x.BatchSize).Returns(ten);
			_configuration.Setup(x => x.TotalRecordsCount).Returns(ten);

			_batchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch) null);

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);

			_batchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}
	}
}