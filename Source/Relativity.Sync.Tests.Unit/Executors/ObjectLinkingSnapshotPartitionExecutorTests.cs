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
            Instance = new ObjectLinkingSnapshotPartitionExecutor(BatchRepository.Object, InstanceSettings.Object, new EmptyLogger());
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
            InstanceSettings.Setup(x => x.GetSyncBatchSizeAsync(It.IsAny<int>())).ReturnsAsync(1);

            const int ten = 10;
            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(ten);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldCreateBatchesWhenTheyDoNotExistMockConfiguration(int items)
        {
            InstanceSettings.Setup(x => x.GetSyncBatchSizeAsync(It.IsAny<int>())).ReturnsAsync(1);

            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(items);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldAddMissingBatchesMockConfiguration()
        {
            const int totalItems = 40;
            const int batchSize = 30;

            InstanceSettings.Setup(x => x.GetSyncBatchSizeAsync(It.IsAny<int>())).ReturnsAsync(batchSize);

            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(totalItems);

            return configuration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration ItShouldSucceedWhenNoMoreBatchesIsRequiredMockConfiguration(int itemsSize)
        {
            InstanceSettings.Setup(x => x.GetSyncBatchSizeAsync(It.IsAny<int>())).ReturnsAsync(itemsSize);

            Mock<IObjectLinkingSnapshotPartitionConfiguration> configuration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();

            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configuration.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configuration.Setup(x => x.TotalRecordsCount).Returns(itemsSize);

            return configuration.Object;
        }

    }
}