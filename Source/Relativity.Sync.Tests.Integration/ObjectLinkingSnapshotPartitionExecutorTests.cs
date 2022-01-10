using Autofac;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal class ObjectLinkingSnapshotPartitionExecutorTests : SnapshotPartitionExecutorTestsBase<IObjectLinkingSnapshotPartitionConfiguration>
	{
        public override void SetUp()
        {
            ContainerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            ContainerBuilder.RegisterType<ObjectLinkingSnapshotPartitionExecutor>().As<IExecutor<IObjectLinkingSnapshotPartitionConfiguration>>();
            base.SetUp();
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration GetSnapshotPartitionConfigurationMock()
        {
            Mock<IObjectLinkingSnapshotPartitionConfiguration> snapshotPartitionConfiguration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>(MockBehavior.Loose);

            return snapshotPartitionConfiguration.Object;
        }

        protected override IObjectLinkingSnapshotPartitionConfiguration GetSnapshotPartitionConfigurationMockAndSetup(int batchSize, int totalRecords)
        {
            Mock<IObjectLinkingSnapshotPartitionConfiguration> snapshotPartitionConfiguration = new Mock<IObjectLinkingSnapshotPartitionConfiguration>(MockBehavior.Loose);

            snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
            snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(totalRecords);

            return snapshotPartitionConfiguration.Object;
        }
    }
}