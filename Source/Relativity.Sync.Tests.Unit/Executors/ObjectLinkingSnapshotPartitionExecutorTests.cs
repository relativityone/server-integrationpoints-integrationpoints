using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class ObjectLinkingSnapshotPartitionExecutorTests : SnapshotPartitionExecutorTestsBase<IObjectLinkingSnapshotPartitionConfiguration>
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Instance = new ObjectLinkingSnapshotPartitionExecutor(BatchRepository.Object, new EmptyLogger());
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration GetConfiguration()
        {
            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();
            
            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldReturnFailureWhenUnableToCreateBatchMockConfiguration()
        {
            const int ten = 10;
            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(ten);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(1);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldCreateBatchesWhenTheyDoNotExistMockConfiguration(int items)
        {
            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(items);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(1);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldAddMissingBatchesMockConfiguration()
        {
            const int totalItems = 40;
            const int batchSize = 30;

            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(totalItems);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(batchSize);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldSucceedWhenNoMoreBatchesIsRequiredMockConfiguration(int itemsSize)
        {
            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(itemsSize);
            configuration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(itemsSize);

            return configuration.Object;
        }

    }
}