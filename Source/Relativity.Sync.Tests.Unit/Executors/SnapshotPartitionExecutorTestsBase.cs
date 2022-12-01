using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal abstract class SnapshotPartitionExecutorTestsBase<T> where T : ISnapshotPartitionConfiguration
    {
        protected SnapshotPartitionExecutorBase Instance;

        protected Mock<IBatchRepository> BatchRepository;

        protected const int _WORKSPACE_ID = 589632;
        protected const int _SYNC_CONF_ID = 214563;
        protected const int _SYNC_BATCH_SIZE = 25000;

        [SetUp]
        public virtual void SetUp()
        {
            BatchRepository = new Mock<IBatchRepository>();
        }

        [Test]
        public async Task ItShouldReturnFailureWhenCannotReadLastBatch()
        {
            BatchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).Throws<InvalidOperationException>();

            T configuration = GetConfiguration();

            // ACT
            ExecutionResult result = await Instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().BeOfType<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldReturnFailureWhenUnableToCreateBatch()
        {
            T configuration = ItShouldReturnFailureWhenUnableToCreateBatchMockConfiguration();

            IBatch batch = null;
            BatchRepository.SetupSequence(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(batch).Throws<InvalidOperationException>();

            // ACT
            ExecutionResult result = await Instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().BeOfType<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldCreateBatchesWhenTheyDoNotExist()
        {
            BatchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync((IBatch)null);

            const int items = 10;
            T configuration = ItShouldCreateBatchesWhenTheyDoNotExistMockConfiguration(items);

            BatchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch)null);

            // ACT
            ExecutionResult result = await Instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Completed);

            for (int i = 0; i < items; i++)
            {
                int index = i;
                BatchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), 1, index));
            }
        }

        [Test]
        public async Task ItShouldAddMissingBatches()
        {
            const int lastStartingIndex = 5;
            const int lastBatchSize = 10;
            const int indexToStartFrom = 15;
            const int itemsLeft = 25;

            Mock<IBatch> lastBatch = new Mock<IBatch>();
            lastBatch.Setup(x => x.StartingIndex).Returns(lastStartingIndex);
            lastBatch.Setup(x => x.TotalDocumentsCount).Returns(lastBatchSize);

            BatchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(lastBatch.Object);

            T configuration = ItShouldAddMissingBatchesMockConfiguration();

            BatchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch)null);

            // ACT
            ExecutionResult result = await Instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Completed);

            BatchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), itemsLeft, indexToStartFrom));
        }

        [Test]
        public async Task ItShouldSucceedWhenNoMoreBatchesIsRequired()
        {
            const int ten = 10;
            Mock<IBatch> lastBatch = new Mock<IBatch>();
            lastBatch.Setup(x => x.StartingIndex).Returns(ten);
            lastBatch.Setup(x => x.TotalDocumentsCount).Returns(ten);

            BatchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(lastBatch.Object);

            T configuration = ItShouldSucceedWhenNoMoreBatchesIsRequiredMockConfiguration(ten);

            BatchRepository.Setup(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((IBatch)null);

            // ACT
            ExecutionResult result = await Instance.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(ExecutionStatus.Completed);

            BatchRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        protected virtual T GetConfiguration()
        {
            Mock<ISnapshotPartitionConfiguration> configuration = new Mock<ISnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);

            return (T)configuration.Object;
        }

        protected virtual T ItShouldReturnFailureWhenUnableToCreateBatchMockConfiguration()
        {
            const int ten = 10;
            Mock<ISnapshotPartitionConfiguration> configuration = new Mock<ISnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(ten);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(1);

            return (T)configuration.Object;
        }

        protected virtual T ItShouldCreateBatchesWhenTheyDoNotExistMockConfiguration(int items)
        {
            Mock<ISnapshotPartitionConfiguration> configuration = new Mock<ISnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(items);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(1);

            return (T)configuration.Object;
        }

        protected virtual T ItShouldAddMissingBatchesMockConfiguration()
        {
            const int totalItems = 40;
            const int batchSize = 30;

            Mock<ISnapshotPartitionConfiguration> configuration = new Mock<ISnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(totalItems);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(batchSize);

            return (T)configuration.Object;
        }

        protected virtual T ItShouldSucceedWhenNoMoreBatchesIsRequiredMockConfiguration(int itemsSize)
        {
            Mock<ISnapshotPartitionConfiguration> configuration = new Mock<ISnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(itemsSize);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(itemsSize);

            return (T)configuration.Object;
        }
    }
}
