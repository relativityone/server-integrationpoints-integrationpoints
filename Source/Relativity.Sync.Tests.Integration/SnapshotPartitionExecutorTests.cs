using Autofac;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal class SnapshotPartitionExecutorTests : SnapshotPartitionExecutorTestsBase<ISnapshotPartitionConfiguration>
	{
        public override void SetUp()
        {
            ContainerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            ContainerBuilder.RegisterType<SnapshotPartitionExecutor>().As<IExecutor<ISnapshotPartitionConfiguration>>();
            base.SetUp();
        }
    }
}