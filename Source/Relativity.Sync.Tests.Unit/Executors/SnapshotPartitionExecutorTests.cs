using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class SnapshotPartitionExecutorTests : SnapshotPartitionExecutorTestsBase<ISnapshotPartitionConfiguration>
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Instance = new SnapshotPartitionExecutor(BatchRepository.Object, new EmptyLogger());
        }
    }
}
