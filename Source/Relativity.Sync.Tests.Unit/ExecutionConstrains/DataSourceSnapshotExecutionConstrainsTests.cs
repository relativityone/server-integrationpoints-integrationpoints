using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public sealed class DataSourceSnapshotExecutionConstrainsTests
    {
        private DataSourceSnapshotExecutionConstrains _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new DataSourceSnapshotExecutionConstrains();
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public async Task ItShouldPreventExecutionIfSnapshotExists(bool snapshotExists, bool expectedCanExecute)
        {
            Mock<IDataSourceSnapshotConfiguration> configuration = new Mock<IDataSourceSnapshotConfiguration>();

            configuration.Setup(x => x.IsSnapshotCreated).Returns(snapshotExists);

            // ACT
            bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            canExecute.Should().Be(expectedCanExecute);
        }
    }
}
